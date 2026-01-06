namespace MechanicBuddy.Management.Api.Domain;

public class Tenant
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Tier { get; set; } = "free"; // free, starter, professional, enterprise
    public string Status { get; set; } = "active"; // active, suspended, cancelled, demo
    public string OwnerEmail { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? CustomDomain { get; set; }
    public bool DomainVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? SubscriptionEndsAt { get; set; }
    public DateTime? LastBilledAt { get; set; }
    public int MaxMechanics { get; set; } = 1;
    public int MaxStorage { get; set; } = 1024; // MB
    public bool IsDemo { get; set; }
    public string? K8sNamespace { get; set; }
    public string? DbConnectionString { get; set; }
    public string? ApiUrl { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
