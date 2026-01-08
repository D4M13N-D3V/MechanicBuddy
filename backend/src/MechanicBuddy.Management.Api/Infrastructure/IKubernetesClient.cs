namespace MechanicBuddy.Management.Api.Infrastructure;

public interface IKubernetesClient
{
    Task<string> CreateNamespaceAsync(string tenantId);
    Task<string> DeployTenantInstanceAsync(string tenantId, string tier);
    Task<string> CreateTenantDatabaseAsync(string tenantId);
    Task ScaleTenantInstanceAsync(string tenantId, int replicas);
    Task DeleteNamespaceAsync(string tenantId);
    Task RestartDeploymentAsync(string tenantId, string deploymentType);
    Task<string> RunMigrationJobAsync(string tenantId);
    Task UpdateTenantTierAsync(string tenantId, string tier);
}
