namespace MechanicBuddy.Management.Api.Infrastructure;

public interface ITenantDatabaseProvisioner
{
    /// <summary>
    /// Creates a new tenant database schema and runs all migrations on the default PostgreSQL host.
    /// </summary>
    /// <param name="tenantId">The tenant identifier (e.g., "testt")</param>
    /// <returns>The connection string for the tenant database</returns>
    Task<string> ProvisionTenantDatabaseAsync(string tenantId);

    /// <summary>
    /// Creates a new tenant database schema on a specific PostgreSQL host.
    /// Used for shared free-tier instances where databases are hosted on a shared cluster.
    /// </summary>
    /// <param name="tenantId">The tenant identifier (e.g., "testt")</param>
    /// <param name="targetPostgresHost">Target PostgreSQL host (null = use default)</param>
    /// <param name="targetPostgresPort">Target PostgreSQL port (null = use default)</param>
    /// <param name="ownerEmail">Owner's email address for the admin account (null = use default)</param>
    /// <param name="ownerName">Owner's name for the admin account (null = use default)</param>
    /// <returns>The connection string for the tenant database</returns>
    Task<string> ProvisionTenantDatabaseAsync(string tenantId, string? targetPostgresHost, int? targetPostgresPort, string? ownerEmail = null, string? ownerName = null);

    /// <summary>
    /// Deletes a tenant's database schema and all associated data from the default PostgreSQL host.
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    Task DeleteTenantDatabaseAsync(string tenantId);

    /// <summary>
    /// Deletes a tenant's database schema from a specific PostgreSQL host.
    /// Used for shared free-tier instances where databases are hosted on a shared cluster.
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    /// <param name="targetPostgresHost">Target PostgreSQL host (null = use default)</param>
    /// <param name="targetPostgresPort">Target PostgreSQL port (null = use default)</param>
    Task DeleteTenantDatabaseAsync(string tenantId, string? targetPostgresHost, int? targetPostgresPort);

    /// <summary>
    /// Checks if a tenant database schema exists on the default PostgreSQL host.
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    Task<bool> TenantDatabaseExistsAsync(string tenantId);

    /// <summary>
    /// Checks if a tenant database schema exists on a specific PostgreSQL host.
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    /// <param name="targetPostgresHost">Target PostgreSQL host (null = use default)</param>
    /// <param name="targetPostgresPort">Target PostgreSQL port (null = use default)</param>
    Task<bool> TenantDatabaseExistsAsync(string tenantId, string? targetPostgresHost, int? targetPostgresPort);

    /// <summary>
    /// Disables all non-admin users for a tenant by setting validated = false.
    /// Used when downgrading from team tier to solo.
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    /// <returns>Number of users disabled</returns>
    Task<int> DisableNonAdminUsersAsync(string tenantId);
}
