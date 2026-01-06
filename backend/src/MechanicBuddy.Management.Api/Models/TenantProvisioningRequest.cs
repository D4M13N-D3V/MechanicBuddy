using System.ComponentModel.DataAnnotations;

namespace MechanicBuddy.Management.Api.Models;

/// <summary>
/// Request to provision a new tenant.
/// </summary>
public class TenantProvisioningRequest
{
    /// <summary>
    /// Company name (will be used to generate tenant ID if not provided).
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Optional custom tenant ID (must be unique, lowercase, alphanumeric with hyphens).
    /// If not provided, will be generated from company name.
    /// </summary>
    [RegularExpression(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$",
        ErrorMessage = "Tenant ID must be lowercase, alphanumeric with hyphens, and cannot start/end with a hyphen")]
    [StringLength(30, MinimumLength = 3)]
    public string? TenantId { get; set; }

    /// <summary>
    /// Owner email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string OwnerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Owner first name.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string OwnerFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Owner last name.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string OwnerLastName { get; set; } = string.Empty;

    /// <summary>
    /// Subscription tier: demo, free, professional, enterprise.
    /// </summary>
    [Required]
    [RegularExpression(@"^(demo|free|professional|enterprise)$")]
    public string SubscriptionTier { get; set; } = "free";

    /// <summary>
    /// Custom domain (optional). If not provided, uses {tenantId}.{baseDomain}.
    /// </summary>
    [RegularExpression(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$",
        ErrorMessage = "Invalid domain format")]
    public string? CustomDomain { get; set; }

    /// <summary>
    /// Stripe customer ID (for paid subscriptions).
    /// </summary>
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Stripe subscription ID (for paid subscriptions).
    /// </summary>
    public string? StripeSubscriptionId { get; set; }

    /// <summary>
    /// Populate demo/sample data after provisioning.
    /// </summary>
    public bool PopulateSampleData { get; set; } = false;

    /// <summary>
    /// Additional environment variables to pass to the API container.
    /// </summary>
    public Dictionary<string, string>? AdditionalEnvVars { get; set; }

    /// <summary>
    /// Override resource limits (for enterprise custom deployments).
    /// </summary>
    public TenantResourceOverrides? ResourceOverrides { get; set; }
}

/// <summary>
/// Resource overrides for custom deployments.
/// </summary>
public class TenantResourceOverrides
{
    public int? PostgresInstances { get; set; }
    public string? PostgresStorageSize { get; set; }
    public int? ApiReplicas { get; set; }
    public int? WebReplicas { get; set; }
    public int? MechanicLimit { get; set; }
    public string? StorageClass { get; set; }
}
