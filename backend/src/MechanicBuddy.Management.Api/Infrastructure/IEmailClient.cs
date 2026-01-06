namespace MechanicBuddy.Management.Api.Infrastructure;

public interface IEmailClient
{
    Task SendDemoRequestConfirmationAsync(string email, string companyName);
    Task SendDemoApprovedEmailAsync(string email, string companyName, string apiUrl, string tenantId, DateTime expiresAt);
    Task SendDemoRejectedEmailAsync(string email, string reason);
    Task SendWelcomeEmailAsync(string email, string companyName, string apiUrl);
    Task SendDemoExpiringSoonEmailAsync(string email, string companyName, string apiUrl, DateTime expiresAt, string tenantId);
    Task SendDemoExpiredEmailAsync(string email, string companyName, string conversionUrl);
}
