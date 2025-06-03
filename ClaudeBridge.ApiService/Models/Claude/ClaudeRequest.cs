using System.Collections.Immutable;

public class ClaudeRequest
{
    public string Model { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public ImmutableArray<ClaudeMessage> Messages { get; set; } = [];
    public double? Temperature { get; set; }
    public bool? Stream { get; set; }
    public string? System { get; set; } = string.Empty;
    public ImmutableArray<McpServer> McpServers { get; set; } = [];
}