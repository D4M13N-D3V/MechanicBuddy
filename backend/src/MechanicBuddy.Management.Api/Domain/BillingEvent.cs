namespace MechanicBuddy.Management.Api.Domain;

public class BillingEvent
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // subscription_created, payment_succeeded, payment_failed, etc.
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? StripeEventId { get; set; }
    public string? InvoiceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
}
