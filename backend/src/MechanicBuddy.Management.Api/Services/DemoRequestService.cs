using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;
using MechanicBuddy.Management.Api.Infrastructure;

namespace MechanicBuddy.Management.Api.Services;

public class DemoRequestService
{
    private readonly IDemoRequestRepository _demoRequestRepository;
    private readonly TenantService _tenantService;
    private readonly IEmailClient _emailClient;
    private readonly ILogger<DemoRequestService> _logger;

    public DemoRequestService(
        IDemoRequestRepository demoRequestRepository,
        TenantService tenantService,
        IEmailClient emailClient,
        ILogger<DemoRequestService> logger)
    {
        _demoRequestRepository = demoRequestRepository;
        _tenantService = tenantService;
        _emailClient = emailClient;
        _logger = logger;
    }

    public async Task<DemoRequest> CreateRequestAsync(string email, string companyName, string? phoneNumber = null)
    {
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

        // Create demo tenant
        var tenant = await _tenantService.CreateTenantAsync(
            demoRequest.CompanyName,
            demoRequest.Email,
            demoRequest.CompanyName,
            tier: "free",
            isDemo: true
        );

        // Update demo request
        demoRequest.Status = "approved";
        demoRequest.TenantId = tenant.TenantId;
        demoRequest.ApprovedAt = DateTime.UtcNow;
        demoRequest.ExpiresAt = DateTime.UtcNow.AddDays(14);
        demoRequest.Notes = notes;

        await _demoRequestRepository.UpdateAsync(demoRequest);

        // Send approval email with credentials
        await _emailClient.SendDemoApprovedEmailAsync(
            demoRequest.Email,
            demoRequest.CompanyName,
            tenant.ApiUrl ?? "",
            tenant.TenantId,
            demoRequest.ExpiresAt.Value
        );

        _logger.LogInformation("Approved demo request {Id}, created tenant {TenantId}", id, tenant.TenantId);

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
        var expiredRequests = (await _demoRequestRepository.GetByStatusAsync("approved"))
            .Where(r => r.ExpiresAt.HasValue && r.ExpiresAt.Value < DateTime.UtcNow)
            .ToList();

        var cleanedCount = 0;

        foreach (var request in expiredRequests)
        {
            if (request.TenantId != null)
            {
                try
                {
                    await _tenantService.DeleteTenantAsync(request.TenantId);
                    request.Status = "expired";
                    await _demoRequestRepository.UpdateAsync(request);
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

    public async Task<int> GetPendingCountAsync()
    {
        return await _demoRequestRepository.GetPendingCountAsync();
    }
}
