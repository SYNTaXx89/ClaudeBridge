using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

public class OpenAIEndpoints
{
    /// <summary>
    /// Call the Claude API to generate chat completions based on the OpenAI ChatCompletionRequest format.
    /// </summary>
    public static async Task<IResult> ChatCompletions(HttpContext context, 
        [FromBody] ChatCompletionRequest request, 
        IOpenAIEndpointsHandler handler,
        ILogger<OpenAIEndpoints> logger)
    {
        try
        {
            return await handler.ChatCompletion(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing chat completion request");

            return Results.Json(new ErrorResponse
            {
                Error = new Error
                {
                    Message = "Internal server error",
                    Type = "server_error"
                }
            }, statusCode: 500);
        }
    }
}
