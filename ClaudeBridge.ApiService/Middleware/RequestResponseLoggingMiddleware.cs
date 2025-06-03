using System.Text;
using System.Text.Json;

namespace ClaudeBridge.ApiService.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log request
        var requestBody = await ReadRequestBodyAsync(context.Request);
        _logger.LogInformation("Request: {Method} {Path}\nBody: {Body}", 
            context.Request.Method, 
            context.Request.Path,
            requestBody);

        // Capture the original response body stream
        var originalBodyStream = context.Response.Body;

        // Create a new memory stream to capture the response
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            // Continue down the middleware pipeline
            await _next(context);

            // Log response
            var response = await ReadResponseBodyAsync(context.Response);
            _logger.LogInformation("Response: {StatusCode}\nBody: {Body}", 
                context.Response.StatusCode,
                response);

            // Copy the response back to the original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }

    private async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return text;
    }
} 