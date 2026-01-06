using System.ComponentModel.DataAnnotations;
using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;
using MechanicBuddy.Management.Api.Infrastructure;
using MechanicBuddy.Management.Api.Models;

namespace MechanicBuddy.Management.Api.Services;

public class DemoRequestService
{
    private readonly IDemoRequestRepository _demoRequestRepository;
    private readonly ITenantProvisioningService _tenantProvisioningService;
    private readonly IEmailClient _emailClient;
    private readonly ILogger<DemoRequestService> _logger;

    public DemoRequestService(
        IDemoRequestRepository demoRequestRepository,
        ITenantProvisioningService tenantProvisioningService,
        IEmailClient emailClient,
        ILogger<DemoRequestService> logger)
    {
        _demoRequestRepository = demoRequestRepository;
        _tenantProvisioningService = tenantProvisioningService;
        _emailClient = emailClient;
        _logger = logger;
    }

    public async Task<DemoRequest> CreateRequestAsync(string email, string companyName, string? phoneNumber = null)
    {
        // Validate email format
        if (!new EmailAddressAttribute().IsValid(email))
        {
            throw new InvalidOperationException("Invalid email address format");
        }

        // Validate company name
        if (string.IsNullOrWhiteSpace(companyName) || companyName.Length < 2)
        {
            throw new InvalidOperationException("Company name must be at least 2 characters long");
        }

        // Check for existing pending or active requests
        var existingRequest = await _demoRequestRepository.GetByEmailAsync(email);
        if (existingRequest != null && (existingRequest.Status == "pending" || existingRequest.Status == "approved"))
        {
            throw new InvalidOperationException("A demo request already exists for this email");
        }

        var demoRequest = new DemoRequest
        {
            Email = email,
            CompanyName = companyName,
            PhoneNumber = phoneNumber,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        var id = await _demoRequestRepository.CreateAsync(demoRequest);
        demoRequest.Id = id;

        // Send confirmation email
        await _emailClient.SendDemoRequestConfirmationAsync(email, companyName);

        _logger.LogInformation("Created demo request {Id} for {Email}", id, email);

        return demoRequest;
    }

    public async Task<DemoRequest?> GetByIdAsync(int id)
    {
        return await _demoRequestRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<DemoRequest>> GetAllAsync(int skip = 0, int take = 50)
    {
        return await _demoRequestRepository.GetAllAsync(skip, take);
    }

    public async Task<IEnumerable<DemoRequest>> GetPendingAsync()
    {
        return await _demoRequestRepository.GetByStatusAsync("pending");
    }

    public async Task<DemoRequest> ApproveRequestAsync(int id, string? notes = null)
    {
        var demoRequest = await _demoRequestRepository.GetByIdAsync(id);
        if (demoRequest == null)
        {
            throw new InvalidOperationException("Demo request not found");
        }

        if (demoRequest.Status != "pending")
        {
            throw new InvalidOperationException("Demo request is not pending");
        }

        // Create demo tenant using ITenantProvisioningService
        var provisioningRequest = new TenantProvisioningRequest
        {
            CompanyName = demoRequest.CompanyName,
            OwnerEmail = demoRequest.Email,
            OwnerFirstName = demoRequest.CompanyName.Split(' ').FirstOrDefault() ?? demoRequest.CompanyName,
            OwnerLastName = demoRequest.CompanyName.Split(' ').Skip(1).FirstOrDefault() ?? "",
            SubscriptionTier = "demo",
            PopulateSampleData = true
        };

        // Validate the provisioning request
        var validation = await _tenantProvisioningService.ValidateProvisioningRequestAsync(provisioningRequest);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException($"Provisioning validation failed: {string.Join(", ", validation.Errors)}");
        }

        // Provision the tenant
        var provisioningResult = await _tenantProvisioningService.ProvisionTenantAsync(provisioningRequest);
        if (!provisioningResult.Success)
        {
            throw new InvalidOperationException($"Failed to provision tenant: {provisioningResult.ErrorMessage}");
        }

        // Update demo request
        demoRequest.Status = "approved";
        demoRequest.TenantId = provisioningResult.TenantId;
        demoRequest.ApprovedAt = DateTime.UtcNow;
        demoRequest.ExpiresAt = DateTime.UtcNow.AddDays(7); // 7-day trial as specified
        demoRequest.Notes = notes;

        await _demoRequestRepository.UpdateAsync(demoRequest);

        // Send welcome email with credentials
        await _emailClient.SendWelcomeEmailAsync(
            demoRequest.Email,
            demoRequest.CompanyName,
            provisioningResult.TenantUrl,
            provisioningResult.AdminUsername,
            provisioningResult.AdminPassword,
            demoRequest.ExpiresAt.Value
        );

        _logger.LogInformation("Approved demo request {Id}, created tenant {TenantId}", id, provisioningResult.TenantId);

        return demoRequest;
    }

    public async Task<DemoRequest> RejectRequestAsync(int id, string reason)
    {
        var demoRequest = await _demoRequestRepository.GetByIdAsync(id);
        if (demoRequest == null)
        {
            throw new InvalidOperationException("Demo request not found");
        }

        demoRequest.Status = "rejected";
        demoRequest.RejectionReason = reason;

        await _demoRequestRepository.UpdateAsync(demoRequest);

        // Send rejection email
        await _emailClient.SendDemoRejectedEmailAsync(demoRequest.Email, reason);

        _logger.LogInformation("Rejected demo request {Id}", id);

        return demoRequest;
    }

    public async Task<int> CleanupExpiredDemosAsync()
    {
        // Get expired demos
        var expiredRequests = await _demoRequestRepository.GetExpiredAsync();
        var cleanedCount = 0;

        foreach (var request in expiredRequests)
        {
            if (request.TenantId != null)
            {
                try
                {
                    // Suspend the tenant instead of deleting
                    await _tenantProvisioningService.SuspendTenantAsync(request.TenantId);

                    // Update status to expired
                    request.Status = "expired";
                    await _demoRequestRepository.UpdateAsync(request);

                    // Send demo expired email
                    var conversionUrl = $"https://mechanicbuddy.com/convert-demo/{request.TenantId}";
                    await _emailClient.SendDemoExpiredEmailAsync(
                        request.Email,
                        request.CompanyName,
                        conversionUrl
                    );

                    cleanedCount++;
                    _logger.LogInformation("Cleaned up expired demo {TenantId}", request.TenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cleanup expired demo {TenantId}", request.TenantId);
                }
            }
        }

        return cleanedCount;
    }

    public async Task<int> SendExpiringDemoRemindersAsync()
    {
        // Get demos expiring in the next 2 days that haven't received a reminder
        var expiringRequests = await _demoRequestRepository.GetExpiringSoonAsync(2);
        var sentCount = 0;

        foreach (var request in expiringRequests)
        {
            if (request.TenantId != null && request.ExpiresAt.HasValue)
            {
                try
                {
                    // Get tenant status to get the URL
                    var tenantStatus = await _tenantProvisioningService.GetTenantStatusAsync(request.TenantId);
                    var apiUrl = tenantStatus.TenantUrl ?? $"https://{request.TenantId}.mechanicbuddy.com";

                    // Send expiring soon email
                    await _emailClient.SendDemoExpiringSoonEmailAsync(
                        request.Email,
                        request.CompanyName,
                        apiUrl,
                        request.ExpiresAt.Value,
                        request.TenantId
                    );

                    // Mark that we sent the reminder
                    request.ExpiringSoonEmailSentAt = DateTime.UtcNow;
                    await _demoRequestRepository.UpdateAsync(request);

                    sentCount++;
                    _logger.LogInformation("Sent expiring reminder for demo {TenantId}", request.TenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send expiring reminder for demo {TenantId}", request.TenantId);
                }
            }
        }

        return sentCount;
    }

    public async Task<DemoRequest> ConvertToPaidAsync(int id, string tier)
    {
        // Validate tier
        var validTiers = new[] { "free", "professional", "enterprise" };
        if (!validTiers.Contains(tier))
        {
            throw new InvalidOperationException($"Invalid tier '{tier}'. Must be one of: {string.Join(", ", validTiers)}");
        }

        var demoRequest = await _demoRequestRepository.GetByIdAsync(id);
        if (demoRequest == null)
        {
            throw new InvalidOperationException("Demo request not found");
        }

        if (demoRequest.Status != "approved" && demoRequest.Status != "expired")
        {
            throw new InvalidOperationException($"Cannot convert demo with status '{demoRequest.Status}'. Must be 'approved' or 'expired'");
        }

        if (string.IsNullOrEmpty(demoRequest.TenantId))
        {
            throw new InvalidOperationException("Demo request does not have an associated tenant");
        }

        try
        {
            // Resume tenant if it's suspended (expired)
            if (demoRequest.Status == "expired")
            {
                await _tenantProvisioningService.ResumeTenantAsync(demoRequest.TenantId);
            }

            // Update tenant to new tier
            var updateRequest = new TenantProvisioningRequest
            {
                CompanyName = demoRequest.CompanyName,
                OwnerEmail = demoRequest.Email,
                OwnerFirstName = demoRequest.CompanyName.Split(' ').FirstOrDefault() ?? demoRequest.CompanyName,
                OwnerLastName = demoRequest.CompanyName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                SubscriptionTier = tier
            };

            var updateResult = await _tenantProvisioningService.UpdateTenantAsync(demoRequest.TenantId, updateRequest);
            if (!updateResult.Success)
            {
                throw new InvalidOperationException($"Failed to update tenant: {updateResult.ErrorMessage}");
            }

            // Update demo request - remove expiration, mark as converted
            demoRequest.Status = "converted";
            demoRequest.ExpiresAt = null;
            demoRequest.Notes = $"{demoRequest.Notes ?? ""}\nConverted to {tier} tier on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC".Trim();

            await _demoRequestRepository.UpdateAsync(demoRequest);

            _logger.LogInformation("Converted demo {TenantId} from demo to {Tier} tier", demoRequest.TenantId, tier);

            return demoRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert demo {TenantId} to paid", demoRequest.TenantId);
            throw;
        }
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _demoRequestRepository.GetPendingCountAsync();
    }
}
