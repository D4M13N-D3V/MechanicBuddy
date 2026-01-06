namespace MechanicBuddy.Management.Api.Infrastructure;

/// <summary>
/// A no-op implementation of IKubernetesClient for non-Kubernetes environments.
/// Logs warnings when methods are called but doesn't fail.
/// Database provisioning still works via the provisioner.
/// </summary>
public class NoOpKubernetesClient : IKubernetesClient
{
    private readonly ILogger<NoOpKubernetesClient> _logger;
    private readonly ITenantDatabaseProvisioner? _dbProvisioner;
    private readonly string _baseDomain;

    public NoOpKubernetesClient(
        ILogger<NoOpKubernetesClient> logger,
        IConfiguration configuration,
        ITenantDatabaseProvisioner? dbProvisioner = null)
    {
        _logger = logger;
        _dbProvisioner = dbProvisioner;
        _baseDomain = configuration["Cloudflare:BaseDomain"] ?? "mechanicbuddy.app";
    }

    public Task<string> CreateNamespaceAsync(string tenantId)
    {
        _logger.LogWarning("Kubernetes not available. Skipping namespace creation for tenant {TenantId}", tenantId);
        return Task.FromResult($"mb-{tenantId}");
    }

    public Task<string> DeployTenantInstanceAsync(string tenantId, string tier)
    {
        _logger.LogWarning("Kubernetes not available. Skipping deployment for tenant {TenantId}", tenantId);
        // Return the external URL format even in dev mode for consistency
        return Task.FromResult($"https://{tenantId}.{_baseDomain}");
    }

    public async Task<string> CreateTenantDatabaseAsync(string tenantId)
    {
        if (_dbProvisioner != null)
        {
            _logger.LogInformation("Provisioning database for tenant {TenantId} (Kubernetes not available)", tenantId);
            return await _dbProvisioner.ProvisionTenantDatabaseAsync(tenantId);
        }

        _logger.LogWarning("Database provisioner not available. Returning mock connection string for tenant {TenantId}", tenantId);
        var schemaName = $"tenant_{tenantId.Replace("-", "_")}";
        return $"Host=localhost;Database=mechanicbuddy;SearchPath={schemaName}";
    }

    public Task ScaleTenantInstanceAsync(string tenantId, int replicas)
    {
        _logger.LogWarning("Kubernetes not available. Skipping scale operation for tenant {TenantId}", tenantId);
        return Task.CompletedTask;
    }

    public Task DeleteNamespaceAsync(string tenantId)
    {
        _logger.LogWarning("Kubernetes not available. Skipping namespace deletion for tenant {TenantId}", tenantId);
        return Task.CompletedTask;
    }
}
