public class ClaudeResponse
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<ClaudeContent> Content { get; set; } = new();
    public string Model { get; set; } = string.Empty;
    public string? StopReason { get; set; }
    public string? StopSequence { get; set; }
    public ClaudeUsage Usage { get; set; } = new();
}