namespace MechanicBuddy.Management.Api.Infrastructure;

public interface ITenantDatabaseProvisioner
{
    /// <summary>
    /// Creates a new tenant database schema and runs all migrations.
    /// </summary>
    /// <param name="tenantId">The tenant identifier (e.g., "testt")</param>
    /// <returns>The connection string for the tenant database</returns>
    Task<string> ProvisionTenantDatabaseAsync(string tenantId);

    /// <summary>
    /// Deletes a tenant's database schema and all associated data.
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    Task DeleteTenantDatabaseAsync(string tenantId);

    /// <summary>
    /// Checks if a tenant database schema exists.
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    Task<bool> TenantDatabaseExistsAsync(string tenantId);
}
