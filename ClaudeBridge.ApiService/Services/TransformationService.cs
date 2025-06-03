using System.Collections.Immutable;
using System.Text.Json;

public class TransformationService
{
    public ClaudeRequest TransformRequest(ChatCompletionRequest openAIRequest, ImmutableArray<McpServer> mcpEndpoints)
    {
        var claudeRequest = new ClaudeRequest
        {
            Model = openAIRequest.Model,
            MaxTokens = openAIRequest.MaxTokens ?? 4096,
            Temperature = openAIRequest.Temperature,
            Stream = openAIRequest.Stream ?? false
        };

        // Extract system message if present
        var systemMessage = openAIRequest.Messages.FirstOrDefault(m => m.Role == "system");
        if (systemMessage != null)
        {
            claudeRequest.System = systemMessage.Content;
        }

        // Convert non-system messages
        claudeRequest.Messages = [
            ..openAIRequest.Messages
                .Where(m => m.Role != "system")
                .Select(msg => new ClaudeMessage
                {
                    Role = msg.Role == "assistant" ? "assistant" : "user",
                    Content = msg.Content
                })
        ];

        claudeRequest.McpServers = mcpEndpoints;

        return claudeRequest;
    }

    private bool _isFirstChunk = true;
    private int _totalTokens = 0;

    public async IAsyncEnumerable<ChatCompletionResponse> TransformStreamingResponse(Stream responseStream, string requestId, string model)
    {
        using var reader = new StreamReader(responseStream);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data: ")) continue;

            var data = line.Substring(6);
            if (data == "[DONE]")
            {
                // Send final chunk with usage and finish reason
                yield return new ChatCompletionResponse
                {
                    Id = requestId,
                    Object = "chat.completion.chunk",
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Model = model,
                    Choices = new List<ChatChoice>
                    {
                        new ChatChoice
                        {
                            Index = 0,
                            Delta = new ChatMessage(),
                            FinishReason = "stop"
                        }
                    },
                    Usage = new Usage
                    {
                        PromptTokens = 0, // TODO: Get actual prompt tokens
                        CompletionTokens = _totalTokens,
                        TotalTokens = _totalTokens
                    }
                };
                yield break;
            }

            var streamResponse = await TryDeserializeStreamResponse(data);
            if (streamResponse != null && streamResponse.Type == "content_block_delta")
            {
                _totalTokens += streamResponse.Delta.Text.Length / 4; // Rough estimate of tokens

                var delta = new ChatMessage();
                if (_isFirstChunk)
                {
                    delta.Role = "assistant";
                    _isFirstChunk = false;
                }
                delta.Content = streamResponse.Delta.Text;

                yield return new ChatCompletionResponse
                {
                    Id = requestId,
                    Object = "chat.completion.chunk",
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Model = model,
                    Choices = new List<ChatChoice>
                    {
                        new ChatChoice
                        {
                            Index = streamResponse.Index,
                            Delta = delta,
                            FinishReason = null
                        }
                    },
                    Usage = null
                };
            }
        }
    }

    private async Task<ClaudeStreamResponse?> TryDeserializeStreamResponse(string data)
    {
        try
        {
            return JsonSerializer.Deserialize<ClaudeStreamResponse>(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
        }
        catch
        {
            return null;
        }
    }

    public ChatCompletionResponse TransformResponse(ClaudeResponse claudeResponse, string requestId, string model)
    {
        var openAIResponse = new ChatCompletionResponse
        {
            Id = requestId,
            Object = "chat.completion",
            Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Model = model,
            Usage = new Usage
            {
                PromptTokens = claudeResponse.Usage.InputTokens,
                CompletionTokens = claudeResponse.Usage.OutputTokens,
                TotalTokens = claudeResponse.Usage.InputTokens + claudeResponse.Usage.OutputTokens
            }
        };

        var content = claudeResponse.Content.FirstOrDefault(c => c.Type == "text")?.Text ?? "";
        var finishReason = MapFinishReason(claudeResponse.StopReason);

        openAIResponse.Choices.Add(new ChatChoice
        {
            Index = 0,
            Message = new ChatMessage
            {
                Role = "assistant",
                Content = content
            },
            FinishReason = finishReason
        });

        return openAIResponse;
    }

    public ErrorResponse TransformError(ClaudeErrorResponse claudeError)
    {
        return new ErrorResponse
        {
            Error = new Error
            {
                Message = claudeError.Error.Message,
                Type = MapErrorType(claudeError.Error.Type),
                Code = claudeError.Error.Type
            }
        };
    }

    private string? MapFinishReason(string? claudeStopReason)
    {
        return claudeStopReason switch
        {
            "end_turn" => "stop",
            "max_tokens" => "length",
            "stop_sequence" => "stop",
            _ => "stop"
        };
    }

    private string MapErrorType(string claudeErrorType)
    {
        return claudeErrorType switch
        {
            "invalid_request_error" => "invalid_request_error",
            "authentication_error" => "invalid_api_key",
            "permission_error" => "insufficient_quota",
            "not_found_error" => "model_not_found",
            "rate_limit_error" => "rate_limit_exceeded",
            "api_error" => "server_error",
            "overloaded_error" => "server_error",
            _ => "server_error"
        };
    }
}