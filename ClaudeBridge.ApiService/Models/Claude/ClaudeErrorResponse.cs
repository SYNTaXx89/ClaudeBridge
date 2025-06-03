public class ClaudeErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public ClaudeError Error { get; set; } = new();
}