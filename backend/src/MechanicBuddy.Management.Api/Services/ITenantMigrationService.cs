namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Interface for tenant migration operations.
/// Handles migrating tenants between shared and dedicated deployments.
/// </summary>
public interface ITenantMigrationService
{
    /// <summary>
    /// Migrates a tenant from dedicated infrastructure to shared free-tier instance.
    /// Used for consolidating existing free-tier tenants onto shared infrastructure.
    /// </summary>
    /// <param name="tenantId">Tenant ID to migrate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Migration result.</returns>
    Task<MigrationResult> MigrateToSharedInstanceAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Migrates a tenant from shared instance to dedicated infrastructure.
    /// Used when upgrading from free tier to a paid tier.
    /// </summary>
    /// <param name="tenantId">Tenant ID to migrate.</param>
    /// <param name="targetTier">Target subscription tier (e.g., "professional", "enterprise").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Migration result.</returns>
    Task<MigrationResult> MigrateToDedicatedInstanceAsync(
        string tenantId,
        string targetTier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Migrates multiple tenants to shared instance in bulk.
    /// </summary>
    /// <param name="tenantIds">Tenant IDs to migrate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bulk migration result.</returns>
    Task<BulkMigrationResult> BulkMigrateToSharedInstanceAsync(
        IEnumerable<string> tenantIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a tenant can be migrated to shared instance.
    /// </summary>
    /// <param name="tenantId">Tenant ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if migration is possible.</returns>
    Task<MigrationEligibility> CheckMigrationEligibilityAsync(
        string tenantId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a single tenant migration.
/// </summary>
public class MigrationResult
{
    public string TenantId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string SourceMode { get; set; } = string.Empty;
    public string TargetMode { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
    public List<string> Steps { get; set; } = new();
}

/// <summary>
/// Result of bulk migration operation.
/// </summary>
public class BulkMigrationResult
{
    public int TotalRequested { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public List<MigrationResult> Results { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
}

/// <summary>
/// Migration eligibility check result.
/// </summary>
public class MigrationEligibility
{
    public string TenantId { get; set; } = string.Empty;
    public bool CanMigrate { get; set; }
    public string CurrentMode { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public List<string> Warnings { get; set; } = new();
}
