using System.Text;
using System.Text.Json;

public class OpenAIEndpointsHandler : IOpenAIEndpointsHandler
{
    private readonly TransformationService _transformationService;
    private readonly ClaudeApiService _claudeApiService;
    private readonly ILogger<OpenAIEndpointsHandler> _logger;

    public OpenAIEndpointsHandler(TransformationService transformationService, ClaudeApiService claudeApiService, ILogger<OpenAIEndpointsHandler> logger)
    {
        _transformationService = transformationService;
        _claudeApiService = claudeApiService;
        _logger = logger;
    }

    private ClaudeRequest TransformRequest(ChatCompletionRequest request)
    {
        var claudeRequest = _transformationService.TransformRequest(request);
        _logger.LogInformation("Transformed request for model: {Model}", claudeRequest.Model);

        return claudeRequest;
    }

    public async Task<IResult> ChatCompletion(ChatCompletionRequest request)
    {
        var token = request.Authorization.GetToken(AuthKeys.Claude);

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Authorization header is missing or empty");
            return Results.Unauthorized();
        }

        _logger.LogInformation("Using token: {Token}", token);
        var requestId = $"chatcmpl-{Guid.NewGuid():N}";

        if (request.Stream ?? false)
        {
            // Handle streaming response
            return await ChatCompletionsStream(request, token, requestId);
        }

        // Handle non-streaming response
        return await ChatCompletionsNonStream(request, token, requestId);
    }

    private async Task<IResult> ChatCompletionsStream(ChatCompletionRequest request, string token, string requestId)
    {
        var claudeRequest = TransformRequest(request);
        var stream = await _claudeApiService.SendStreamingRequestAsync(claudeRequest, token);
        if (stream == null)
        {
            return Results.Json(new ErrorResponse
            {
                Error = new Error
                {
                    Message = "Failed to get response from Claude API",
                    Type = "server_error"
                }
            }, statusCode: 500);
        }

        var sseStream = new MemoryStream();
        try
        {
            await foreach (var chunk in _transformationService.TransformStreamingResponse(stream, requestId, claudeRequest.Model))
            {
                var json = JsonSerializer.Serialize(chunk, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                await sseStream.WriteAsync(Encoding.UTF8.GetBytes($"data: {json}\n\n"));
            }
            await sseStream.WriteAsync(Encoding.UTF8.GetBytes("data: [DONE]\n\n"));
            sseStream.Position = 0;
            return Results.Stream(sseStream, "text/event-stream");
        }
        finally
        {
            await stream.DisposeAsync();
        }
    }

    private async Task<IResult> ChatCompletionsNonStream(ChatCompletionRequest request, string token, string requestId)
    {
        var claudeRequest = TransformRequest(request);
        var claudeResponse = await _claudeApiService.SendRequestAsync(claudeRequest, token);

        if (claudeResponse == null)
        {
            return Results.Json(new ErrorResponse
            {
                Error = new Error
                {
                    Message = "Failed to get response from Claude API",
                    Type = "server_error"
                }
            }, statusCode: 500);
        }

        // Transform Claude response to OpenAI format
        var openAIResponse = _transformationService.TransformResponse(claudeResponse, requestId, claudeRequest.Model);

        return Results.Json(openAIResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
    }
}