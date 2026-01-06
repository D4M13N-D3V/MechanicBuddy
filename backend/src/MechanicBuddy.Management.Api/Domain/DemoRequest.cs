namespace MechanicBuddy.Management.Api.Domain;

public class DemoRequest
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Message { get; set; }
    public string? IpAddress { get; set; }
    public string Status { get; set; } = "pending"; // pending, approved, rejected, expired, converted
    public string? TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ExpiringSoonEmailSentAt { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
}
