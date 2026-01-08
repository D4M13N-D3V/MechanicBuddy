using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;
using MechanicBuddy.Management.Api.Infrastructure;

namespace MechanicBuddy.Management.Api.Services;

public class DeleteTenantResult
{
    public bool Success { get; set; }
    public bool KubernetesDeleted { get; set; }
    public bool DatabaseDeleted { get; set; }
    public bool TenantNotInDatabase { get; set; }
    public string? KubernetesError { get; set; }
    public string? DatabaseError { get; set; }
}

public class TenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IKubernetesClient _k8sClient;
    private readonly IEmailClient _emailClient;
    private readonly ILogger<TenantService> _logger;

    private const string DefaultAdminUsername = "admin";
    private const string DefaultAdminPassword = "carcare";

    public TenantService(
        ITenantRepository tenantRepository,
        IKubernetesClient k8sClient,
        IEmailClient emailClient,
        ILogger<TenantService> logger)
    {
        _tenantRepository = tenantRepository;
        _k8sClient = k8sClient;
        _emailClient = emailClient;
        _logger = logger;
    }

    public async Task<Tenant?> GetByIdAsync(int id)
    {
        return await _tenantRepository.GetByIdAsync(id);
    }

    public async Task<Tenant?> GetByTenantIdAsync(string tenantId)
    {
        return await _tenantRepository.GetByTenantIdAsync(tenantId);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(int skip = 0, int take = 50)
    {
        return await _tenantRepository.GetAllAsync(skip, take);
    }

    public async Task<IEnumerable<Tenant>> GetTenantsByOwnerEmailAsync(string ownerEmail)
    {
        return await _tenantRepository.GetByOwnerEmailAsync(ownerEmail);
    }

    public async Task<Tenant> CreateTenantAsync(string companyName, string ownerEmail, string ownerName, string tier = "free", bool isDemo = false)
    {
        // Generate unique tenant ID
        var tenantId = GenerateTenantId(companyName);

        // Ensure uniqueness
        var existing = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (existing != null)
        {
            tenantId = $"{tenantId}-{Guid.NewGuid().ToString()[..8]}";
        }

        var tenant = new Tenant
        {
            TenantId = tenantId,
            CompanyName = companyName,
            OwnerEmail = ownerEmail,
            OwnerName = ownerName,
            Tier = tier,
            Status = isDemo ? "trial" : "active",
            IsDemo = isDemo,
            CreatedAt = DateTime.UtcNow,
            TrialEndsAt = isDemo ? DateTime.UtcNow.AddDays(7) : null, // No expiration for regular signups
            MaxMechanics = GetMaxMechanicsForTier(tier),
            MaxStorage = GetMaxStorageForTier(tier)
        };

        try
        {
            // Create Kubernetes namespace and resources
            var k8sNamespace = await _k8sClient.CreateNamespaceAsync(tenantId);
            tenant.K8sNamespace = k8sNamespace;

            // Deploy tenant instance
            var apiUrl = await _k8sClient.DeployTenantInstanceAsync(tenantId, tier);
            tenant.ApiUrl = apiUrl;

            // Create tenant database
            var dbConnectionString = await _k8sClient.CreateTenantDatabaseAsync(tenantId);
            tenant.DbConnectionString = dbConnectionString;

            // Save to database
            var id = await _tenantRepository.CreateAsync(tenant);
            tenant.Id = id;

            _logger.LogInformation("Created new tenant {TenantId} for {OwnerEmail}", tenantId, ownerEmail);

            // Send welcome email with credentials (non-blocking)
            try
            {
                if (isDemo)
                {
                    await _emailClient.SendWelcomeEmailAsync(
                        ownerEmail,
                        companyName,
                        tenant.ApiUrl ?? $"https://{tenantId}.mechanicbuddy.com",
                        DefaultAdminUsername,
                        DefaultAdminPassword,
                        tenant.TrialEndsAt ?? DateTime.UtcNow.AddDays(7)
                    );
                }
                else
                {
                    await _emailClient.SendAccountCreatedEmailAsync(
                        ownerEmail,
                        companyName,
                        tenant.ApiUrl ?? $"https://{tenantId}.mechanicbuddy.com",
                        DefaultAdminUsername,
                        DefaultAdminPassword,
                        tier
                    );
                }
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send welcome email to {OwnerEmail}, but tenant was created", ownerEmail);
            }

            return tenant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tenant {TenantId}", tenantId);

            // Cleanup on failure
            try
            {
                await _k8sClient.DeleteNamespaceAsync(tenantId);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Failed to cleanup namespace for failed tenant {TenantId}", tenantId);
            }

            throw;
        }
    }

    public async Task<bool> UpdateTenantAsync(Tenant tenant)
    {
        return await _tenantRepository.UpdateAsync(tenant);
    }

    public async Task<bool> SuspendTenantAsync(string tenantId, string reason)
    {
        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            return false;
        }

        tenant.Status = "suspended";
        tenant.Metadata ??= new Dictionary<string, object>();
        tenant.Metadata["suspension_reason"] = reason;
        tenant.Metadata["suspended_at"] = DateTime.UtcNow;

        await _k8sClient.ScaleTenantInstanceAsync(tenantId, 0);

        return await _tenantRepository.UpdateAsync(tenant);
    }

    public async Task<bool> ResumeTenantAsync(string tenantId)
    {
        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            return false;
        }

        tenant.Status = "active";
        tenant.Metadata?.Remove("suspension_reason");
        tenant.Metadata?.Remove("suspended_at");

        await _k8sClient.ScaleTenantInstanceAsync(tenantId, 1);

        return await _tenantRepository.UpdateAsync(tenant);
    }

    public async Task<DeleteTenantResult> DeleteTenantAsync(string tenantId)
    {
        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        var result = new DeleteTenantResult();

        // Always try to delete Kubernetes resources (handles orphaned resources)
        try
        {
            await _k8sClient.DeleteNamespaceAsync(tenantId);
            result.KubernetesDeleted = true;
            _logger.LogInformation("Deleted Kubernetes resources for tenant {TenantId}", tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete Kubernetes resources for tenant {TenantId}", tenantId);
            result.KubernetesError = ex.Message;
        }

        // Delete from database if tenant exists
        if (tenant != null)
        {
            try
            {
                var deleted = await _tenantRepository.DeleteAsync(tenant.Id);
                result.DatabaseDeleted = deleted;
                if (deleted)
                {
                    _logger.LogInformation("Deleted tenant {TenantId} from database", tenantId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete tenant {TenantId} from database", tenantId);
                result.DatabaseError = ex.Message;
            }
        }
        else
        {
            _logger.LogWarning("Tenant {TenantId} not found in database (may be orphaned K8s resources only)", tenantId);
            result.TenantNotInDatabase = true;
        }

        result.Success = result.KubernetesDeleted || result.DatabaseDeleted;
        return result;
    }

    public async Task<Dictionary<string, object>> GetStatsAsync()
    {
        var totalCount = await _tenantRepository.GetTotalCountAsync();
        var countByTier = await _tenantRepository.GetCountByTierAsync();
        var countByStatus = await _tenantRepository.GetCountByStatusAsync();

        return new Dictionary<string, object>
        {
            ["totalTenants"] = totalCount,
            ["byTier"] = countByTier,
            ["byStatus"] = countByStatus
        };
    }

    private static string GenerateTenantId(string companyName)
    {
        // Convert to lowercase, remove special chars, replace spaces with hyphens
        var normalized = companyName.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove non-alphanumeric characters except hyphens
        normalized = new string(normalized.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Ensure it starts with a letter (Kubernetes requirement)
        if (!char.IsLetter(normalized[0]))
        {
            normalized = "t-" + normalized;
        }

        // Limit length
        if (normalized.Length > 20)
        {
            normalized = normalized[..20];
        }

        return normalized;
    }

    private static int GetMaxMechanicsForTier(string tier) => tier switch
    {
        "solo" or "free" => 1,
        "team" => int.MaxValue,      // Unlimited
        "lifetime" => int.MaxValue,  // Unlimited
        _ => 1
    };

    private static int GetMaxStorageForTier(string tier) => tier switch
    {
        "solo" or "free" => 5120,     // 5 GB
        "team" => 102400,             // 100 GB
        "lifetime" => 102400,         // 100 GB
        _ => 5120
    };
}
