using System.Collections.Immutable;

public class MCPService : IMCPService
{
    /// <summary>
    /// TODO: Load from configuration etc.
    /// </summary>
    public ImmutableArray<McpServer> GetMcpServers()
    {
        return ImmutableArray<McpServer>.Empty;
    }
}
