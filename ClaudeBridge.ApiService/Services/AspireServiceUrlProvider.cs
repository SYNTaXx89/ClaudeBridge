using Microsoft.Extensions.ServiceDiscovery;

public class AspireServiceUrlProvider : IServiceUrlProvider
{
    private readonly ServiceEndpointResolver _resolver;
    public AspireServiceUrlProvider(ServiceEndpointResolver resolver)
    {
        _resolver = resolver;
    }

    public async Task<string> GetServiceUrlAsync(string serviceName)
    {
        var endpoints = await _resolver.GetEndpointsAsync(serviceName, CancellationToken.None);
        return endpoints.Endpoints.FirstOrDefault()?.ToString() ??
               throw new InvalidOperationException($"Service '{serviceName}' not found");
    }
}