namespace MechanicBuddy.Management.Api.Infrastructure;

/// <summary>
/// A no-op implementation of IKubernetesClient for non-Kubernetes environments.
/// Logs warnings when methods are called but doesn't fail.
/// </summary>
public class NoOpKubernetesClient : IKubernetesClient
{
    private readonly ILogger<NoOpKubernetesClient> _logger;

    public NoOpKubernetesClient(ILogger<NoOpKubernetesClient> logger)
    {
        _logger = logger;
    }

    public Task<string> CreateNamespaceAsync(string tenantId)
    {
        _logger.LogWarning("Kubernetes not available. Skipping namespace creation for tenant {TenantId}", tenantId);
        return Task.FromResult($"mb-{tenantId}");
    }

    public Task<string> DeployTenantInstanceAsync(string tenantId, string tier)
    {
        _logger.LogWarning("Kubernetes not available. Skipping deployment for tenant {TenantId}", tenantId);
        return Task.FromResult($"http://localhost:15567");
    }

    public Task<string> CreateTenantDatabaseAsync(string tenantId)
    {
        _logger.LogWarning("Kubernetes not available. Returning mock connection string for tenant {TenantId}", tenantId);
        var schemaName = $"tenant_{tenantId.Replace("-", "_")}";
        return Task.FromResult($"Host=localhost;Database=mechanicbuddy;SearchPath={schemaName}");
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
