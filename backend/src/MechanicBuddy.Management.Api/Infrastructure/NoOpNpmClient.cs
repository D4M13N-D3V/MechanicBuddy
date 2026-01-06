namespace MechanicBuddy.Management.Api.Infrastructure;

/// <summary>
/// A no-op implementation of INpmClient for environments without NPM configured.
/// </summary>
public class NoOpNpmClient : INpmClient
{
    private readonly ILogger<NoOpNpmClient> _logger;

    public NoOpNpmClient(ILogger<NoOpNpmClient> logger)
    {
        _logger = logger;
    }

    public Task<bool> CreateProxyHostAsync(string tenantId)
    {
        _logger.LogWarning("NPM not configured. Skipping proxy host creation for tenant {TenantId}", tenantId);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteProxyHostAsync(string tenantId)
    {
        _logger.LogWarning("NPM not configured. Skipping proxy host deletion for tenant {TenantId}", tenantId);
        return Task.FromResult(true);
    }
}
