namespace MechanicBuddy.Management.Api.Domain;

public class DomainVerification
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string VerificationToken { get; set; } = string.Empty;
    public string VerificationMethod { get; set; } = "dns"; // dns, file
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
