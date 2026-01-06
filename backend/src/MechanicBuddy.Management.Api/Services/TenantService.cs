using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;
using MechanicBuddy.Management.Api.Infrastructure;

namespace MechanicBuddy.Management.Api.Services;

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
            TrialEndsAt = isDemo ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddDays(30),
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
                await _emailClient.SendWelcomeEmailAsync(
                    ownerEmail,
                    companyName,
                    tenant.ApiUrl ?? $"https://{tenantId}.mechanicbuddy.com",
                    DefaultAdminUsername,
                    DefaultAdminPassword,
                    tenant.TrialEndsAt ?? DateTime.UtcNow.AddDays(30)
                );
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

    public async Task<bool> DeleteTenantAsync(string tenantId)
    {
        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            return false;
        }

        try
        {
            // Delete Kubernetes resources
            await _k8sClient.DeleteNamespaceAsync(tenantId);

            // Delete from database
            return await _tenantRepository.DeleteAsync(tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete tenant {TenantId}", tenantId);
            throw;
        }
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
        "free" => 1,
        "starter" => 5,
        "professional" => 20,
        "enterprise" => 100,
        _ => 1
    };

    private static int GetMaxStorageForTier(string tier) => tier switch
    {
        "free" => 1024,        // 1 GB
        "starter" => 10240,    // 10 GB
        "professional" => 51200, // 50 GB
        "enterprise" => 512000,  // 500 GB
        _ => 1024
    };
}
