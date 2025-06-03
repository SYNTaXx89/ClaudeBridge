public class ClaudeStreamResponse
{
    public string Type { get; set; } = string.Empty;
    public int Index { get; set; }
    public ClaudeStreamDelta Delta { get; set; } = new();
}