using System.ComponentModel;

/// <summary>
/// Request object for chat completion API
/// </summary>
public class ChatCompletionRequest
{
    /// <summary>
    /// Authorization for each connected service
    /// </summary>
    public Dictionary<string, string> Authorization { get; set; } = [];

    /// <summary>
    /// ID of the model to use. Currently only supports "claude-3-opus-20240229" and "claude-3-sonnet-20240229"
    /// </summary>
    [DefaultValue("claude-4-sonnet-20250514")]
    public string Model { get; set; } = "claude-4-sonnet-20250514";

    /// <summary>
    /// The messages to generate a response for
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = [];

    /// <summary>
    /// What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
    /// </summary>
    [DefaultValue(1.0)]
    public double? Temperature { get; set; } = 1.0;

    /// <summary>
    /// The maximum number of tokens to generate in the completion. Defaults to 4096.
    /// </summary>
    [DefaultValue(20000)]
    public int? MaxTokens { get; set; } = 20000;


    /// <summary>
    /// If set, partial message deltas will be sent, like in ChatGPT. Tokens will be sent as data-only server-sent events as they become available.
    /// </summary>
    [DefaultValue(false)]
    public bool? Stream { get; set; } = false;
}