using System.Text;
using System.Text.Json;

public class ClaudeApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClaudeApiService> _logger;
    private const string ClaudeApiUrl = "https://api.anthropic.com/v1/messages";

    public ClaudeApiService(HttpClient httpClient, ILogger<ClaudeApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ClaudeResponse?> SendRequestAsync(ClaudeRequest request, string authToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", authToken);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Add("anthropic-beta", "mcp-client-2025-04-04");

            var response = await _httpClient.PostAsync(ClaudeApiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude API returned error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return null;
            }

            return JsonSerializer.Deserialize<ClaudeResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude API");
            return null;
        }
    }

    public async Task<Stream?> SendStreamingRequestAsync(ClaudeRequest request, string authToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", authToken);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Add("accept", "text/event-stream");

            var response = await _httpClient.PostAsync(ClaudeApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Claude API returned error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return null;
            }

            return await response.Content.ReadAsStreamAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude API");
            return null;
        }
    }

    public Task<ClaudeErrorResponse?> GetErrorResponseAsync(string authToken, string errorContent)
    {
        try
        {
            var result = JsonSerializer.Deserialize<ClaudeErrorResponse>(errorContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Claude error response");
            var errorResponse = new ClaudeErrorResponse
            {
                Type = "error",
                Error = new ClaudeError
                {
                    Type = "api_error",
                    Message = "Unknown error occurred"
                }
            };
            return Task.FromResult<ClaudeErrorResponse?>(errorResponse);
        }
    }
}