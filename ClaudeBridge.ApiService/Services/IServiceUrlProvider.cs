public interface IServiceUrlProvider
{
    Task<string> GetServiceUrlAsync(string serviceName);
}