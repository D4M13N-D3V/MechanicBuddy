using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MechanicBuddy.Management.Api.Infrastructure;

public class ResendEmailClient : IEmailClient
{
    private readonly HttpClient _httpClient;
    private readonly string _fromEmail;
    private readonly ILogger<ResendEmailClient> _logger;

    public ResendEmailClient(IConfiguration configuration, ILogger<ResendEmailClient> logger)
    {
        _logger = logger;
        var apiKey = configuration["Email:ResendApiKey"]
            ?? throw new InvalidOperationException("Resend API key not configured");

        _fromEmail = configuration["Email:FromEmail"] ?? "noreply@mechanicbuddy.com";

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.resend.com/")
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task SendDemoRequestConfirmationAsync(string email, string companyName)
    {
        var subject = "Demo Request Received - MechanicBuddy";
        var html = $@"
            <h1>Thank you for your interest in MechanicBuddy!</h1>
            <p>Hi {companyName},</p>
            <p>We've received your demo request and our team will review it shortly.</p>
            <p>You'll receive an email within 1-2 business days with access to your demo instance.</p>
            <p>Best regards,<br/>The MechanicBuddy Team</p>
        ";

        await SendEmailAsync(email, subject, html);
    }

    public async Task SendDemoApprovedEmailAsync(string email, string companyName, string apiUrl, string tenantId, DateTime expiresAt)
    {
        var subject = "Your MechanicBuddy Demo is Ready!";
        var html = $@"
            <h1>Welcome to MechanicBuddy!</h1>
            <p>Hi {companyName},</p>
            <p>Great news! Your demo instance is now ready.</p>
            <h2>Access Details:</h2>
            <ul>
                <li><strong>URL:</strong> {apiUrl}</li>
                <li><strong>Tenant ID:</strong> {tenantId}</li>
                <li><strong>Demo expires:</strong> {expiresAt:MMMM dd, yyyy}</li>
            </ul>
            <p>Default login credentials:</p>
            <ul>
                <li><strong>Username:</strong> admin</li>
                <li><strong>Password:</strong> carcare</li>
            </ul>
            <p>Please change your password after first login.</p>
            <p>Your demo includes full access to all features for 14 days.</p>
            <p>If you have any questions, feel free to reach out!</p>
            <p>Best regards,<br/>The MechanicBuddy Team</p>
        ";

        await SendEmailAsync(email, subject, html);
    }

    public async Task SendDemoRejectedEmailAsync(string email, string reason)
    {
        var subject = "Demo Request Update - MechanicBuddy";
        var html = $@"
            <h1>Demo Request Update</h1>
            <p>Thank you for your interest in MechanicBuddy.</p>
            <p>Unfortunately, we're unable to approve your demo request at this time.</p>
            <p><strong>Reason:</strong> {reason}</p>
            <p>If you have any questions or would like to discuss further, please contact our support team.</p>
            <p>Best regards,<br/>The MechanicBuddy Team</p>
        ";

        await SendEmailAsync(email, subject, html);
    }

    public async Task SendWelcomeEmailAsync(string email, string companyName, string apiUrl)
    {
        var subject = "Welcome to MechanicBuddy!";
        var html = $@"
            <h1>Welcome to MechanicBuddy, {companyName}!</h1>
            <p>Your account has been successfully created.</p>
            <h2>Getting Started:</h2>
            <ul>
                <li>Access your instance at: {apiUrl}</li>
                <li>Complete your profile setup</li>
                <li>Add your team members</li>
                <li>Start creating work orders</li>
            </ul>
            <p>Need help? Check out our documentation or contact support.</p>
            <p>Best regards,<br/>The MechanicBuddy Team</p>
        ";

        await SendEmailAsync(email, subject, html);
    }

    public async Task SendDemoExpiringSoonEmailAsync(string email, string companyName, string apiUrl, DateTime expiresAt, string tenantId)
    {
        var daysRemaining = (expiresAt - DateTime.UtcNow).Days;
        var subject = $"Your MechanicBuddy Demo Expires in {daysRemaining} Days";
        var html = $@"
            <h1>Your Demo is Expiring Soon</h1>
            <p>Hi {companyName},</p>
            <p>This is a friendly reminder that your MechanicBuddy demo will expire in <strong>{daysRemaining} days</strong> on <strong>{expiresAt:MMMM dd, yyyy}</strong>.</p>
            <h2>Don't Lose Your Data!</h2>
            <p>If you've been enjoying MechanicBuddy and want to keep using it, you can convert your demo to a paid subscription to:</p>
            <ul>
                <li>Keep all your existing data and work orders</li>
                <li>Continue with uninterrupted service</li>
                <li>Access advanced features and support</li>
                <li>Manage unlimited mechanics and vehicles</li>
            </ul>
            <h2>Upgrade Now</h2>
            <p>Visit your instance and upgrade to a paid plan:</p>
            <p><a href=""{apiUrl}/settings/subscription"" style=""display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 4px;"">Upgrade My Account</a></p>
            <p>Or continue using the demo until it expires on {expiresAt:MMMM dd, yyyy}.</p>
            <h2>Need Help?</h2>
            <p>If you have questions or need assistance upgrading, please don't hesitate to contact our support team.</p>
            <p>Best regards,<br/>The MechanicBuddy Team</p>
        ";

        await SendEmailAsync(email, subject, html);
    }

    public async Task SendDemoExpiredEmailAsync(string email, string companyName, string conversionUrl)
    {
        var subject = "Your MechanicBuddy Demo Has Expired";
        var html = $@"
            <h1>Demo Period Ended</h1>
            <p>Hi {companyName},</p>
            <p>Your MechanicBuddy demo period has come to an end. We hope you enjoyed exploring our platform!</p>
            <h2>Your Demo Has Been Suspended</h2>
            <p>Your demo instance has been temporarily suspended. However, all your data is still safe and can be restored if you upgrade to a paid subscription.</p>
            <h2>Convert to a Paid Plan</h2>
            <p>Keep all your work and continue where you left off by upgrading to a paid plan:</p>
            <ul>
                <li><strong>Starter Plan</strong> - Perfect for small workshops ($29/month)</li>
                <li><strong>Professional Plan</strong> - For growing businesses ($79/month)</li>
                <li><strong>Enterprise Plan</strong> - Custom solutions for large operations</li>
            </ul>
            <p><a href=""{conversionUrl}"" style=""display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 4px;"">Upgrade Now and Keep Your Data</a></p>
            <h2>What Happens Next?</h2>
            <p>Your demo instance will remain suspended for 30 days. After that, the instance and all associated data will be permanently deleted.</p>
            <p>If you have any questions or would like to discuss a custom plan, please contact our sales team.</p>
            <p>Thank you for trying MechanicBuddy!</p>
            <p>Best regards,<br/>The MechanicBuddy Team</p>
        ";

        await SendEmailAsync(email, subject, html);
    }

    private async Task SendEmailAsync(string to, string subject, string html)
    {
        try
        {
            var payload = new
            {
                from = _fromEmail,
                to = new[] { to },
                subject = subject,
                html = html
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("emails", content);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
    }
}
