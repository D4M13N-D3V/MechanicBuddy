namespace MechanicBuddy.Management.Api.Models;

/// <summary>
/// Result of a tenant provisioning operation.
/// </summary>
public class TenantProvisioningResult
{
    /// <summary>
    /// Indicates if the provisioning was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The assigned tenant ID.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// The tenant URL.
    /// </summary>
    public string TenantUrl { get; set; } = string.Empty;

    /// <summary>
    /// API endpoint URL.
    /// </summary>
    public string ApiUrl { get; set; } = string.Empty;

    /// <summary>
    /// Kubernetes namespace where tenant is deployed.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Default admin username.
    /// </summary>
    public string AdminUsername { get; set; } = string.Empty;

    /// <summary>
    /// Default admin password (only returned once).
    /// </summary>
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>
    /// Helm release name.
    /// </summary>
    public string HelmRelease { get; set; } = string.Empty;

    /// <summary>
    /// Subscription tier.
    /// </summary>
    public string SubscriptionTier { get; set; } = string.Empty;

    /// <summary>
    /// Database connection string (for admin purposes).
    /// </summary>
    public string? DatabaseConnectionString { get; set; }

    /// <summary>
    /// Stripe customer ID (if applicable).
    /// </summary>
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Error message if provisioning failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Detailed provisioning log.
    /// </summary>
    public List<ProvisioningLogEntry> ProvisioningLog { get; set; } = new();

    /// <summary>
    /// Timestamp when provisioning started.
    /// </summary>
    public DateTime ProvisionedAt { get; set; }

    /// <summary>
    /// Duration of provisioning operation.
    /// </summary>
    public TimeSpan ProvisioningDuration { get; set; }

    /// <summary>
    /// Expiration date (for demo/trial accounts).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Resource allocation summary.
    /// </summary>
    public ResourceAllocation? Resources { get; set; }
}

/// <summary>
/// Log entry for provisioning steps.
/// </summary>
public class ProvisioningLogEntry
{
    /// <summary>
    /// Timestamp of the log entry.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Log level: Info, Warning, Error.
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Log message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Step name (e.g., "CreateNamespace", "DeployHelm", "WaitForDatabase").
    /// </summary>
    public string? Step { get; set; }

    /// <summary>
    /// Additional context data.
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Resource allocation information.
/// </summary>
public class ResourceAllocation
{
    public int PostgresInstances { get; set; }
    public string PostgresStorage { get; set; } = string.Empty;
    public int ApiReplicas { get; set; }
    public int WebReplicas { get; set; }
    public int? MechanicLimit { get; set; }
    public bool BackupEnabled { get; set; }
}
