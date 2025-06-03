using System.Collections.Immutable;

public interface IMCPService
{
    /// <summary>
    /// TODO: Load from configuration etc.
    /// </summary>
    ImmutableArray<McpServer> GetMcpServers();
}