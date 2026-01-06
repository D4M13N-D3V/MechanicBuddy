namespace MechanicBuddy.Management.Api.Infrastructure;

/// <summary>
/// No-op implementation of ICloudflareClient for local development
/// </summary>
public class NoOpCloudflareClient : ICloudflareClient
{
    private readonly ILogger<NoOpCloudflareClient> _logger;

    public NoOpCloudflareClient(ILogger<NoOpCloudflareClient> logger)
    {
        _logger = logger;
    }

    public Task<bool> CreateTenantDnsRecordAsync(string subdomain, string? target = null)
    {
        _logger.LogWarning("Cloudflare not configured. Skipping DNS record creation for {Subdomain}", subdomain);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteTenantDnsRecordAsync(string subdomain)
    {
        _logger.LogWarning("Cloudflare not configured. Skipping DNS record deletion for {Subdomain}", subdomain);
        return Task.FromResult(true);
    }

    public Task<bool> DnsRecordExistsAsync(string subdomain)
    {
        return Task.FromResult(false);
    }
}
