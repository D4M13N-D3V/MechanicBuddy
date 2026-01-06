using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;
using MechanicBuddy.Management.Api.Authorization;
using BCrypt.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MechanicBuddy.Management.Api.Services;

public class SuperAdminService
{
    private readonly ISuperAdminRepository _superAdminRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IKubernetesClientService _k8sClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SuperAdminService> _logger;

    public SuperAdminService(
        ISuperAdminRepository superAdminRepository,
        ITenantRepository tenantRepository,
        IKubernetesClientService k8sClient,
        IHttpClientFactory httpClientFactory,
        ILogger<SuperAdminService> logger)
    {
        _superAdminRepository = superAdminRepository;
        _tenantRepository = tenantRepository;
        _k8sClient = k8sClient;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SuperAdmin?> AuthenticateAsync(string email, string password)
    {
        var admin = await _superAdminRepository.GetByEmailAsync(email);
        if (admin == null || !admin.IsActive)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", email);
            return null;
        }

        // Update last login
        await _superAdminRepository.UpdateLastLoginAsync(admin.Id);
        admin.LastLoginAt = DateTime.UtcNow;

        _logger.LogInformation("Successful login for {Email}", email);

        return admin;
    }

    public async Task<SuperAdmin> CreateAdminAsync(string email, string password, string name, string role = "admin")
    {
        var existingAdmin = await _superAdminRepository.GetByEmailAsync(email);
        if (existingAdmin != null)
        {
            throw new InvalidOperationException("Admin with this email already exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var admin = new SuperAdmin
        {
            Email = email,
            PasswordHash = passwordHash,
            Name = name,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _superAdminRepository.CreateAsync(admin);
        admin.Id = id;

        _logger.LogInformation("Created new super admin: {Email}", email);

        return admin;
    }

    public async Task<SuperAdmin?> GetByIdAsync(int id)
    {
        return await _superAdminRepository.GetByIdAsync(id);
    }

    public async Task<SuperAdmin?> GetByEmailAsync(string email)
    {
        return await _superAdminRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<SuperAdmin>> GetAllAsync()
    {
        return await _superAdminRepository.GetAllAsync();
    }

    public async Task<bool> UpdatePasswordAsync(int id, string currentPassword, string newPassword)
    {
        var admin = await _superAdminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, admin.PasswordHash))
        {
            return false;
        }

        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        return await _superAdminRepository.UpdateAsync(admin);
    }

    public async Task<bool> DeactivateAdminAsync(int id)
    {
        var admin = await _superAdminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            return false;
        }

        admin.IsActive = false;
        return await _superAdminRepository.UpdateAsync(admin);
    }

    public async Task<bool> ActivateAdminAsync(int id)
    {
        var admin = await _superAdminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            return false;
        }

        admin.IsActive = true;
        return await _superAdminRepository.UpdateAsync(admin);
    }

    public async Task<bool> DeleteAdminAsync(int id)
    {
        return await _superAdminRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Generate a temporary access token to access a tenant's dashboard as super admin.
    /// This creates a special JWT that the tenant API will recognize as a super admin session.
    /// </summary>
    public async Task<TenantAccessResult> GenerateTenantAccessAsync(int superAdminId, string tenantId)
    {
        var admin = await _superAdminRepository.GetByIdAsync(superAdminId);
        if (admin == null || !admin.IsActive)
        {
            return new TenantAccessResult { Success = false, Error = "Super admin not found or inactive" };
        }

        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            return new TenantAccessResult { Success = false, Error = "Tenant not found" };
        }

        try
        {
            // Get the tenant's API internal URL from Kubernetes
            var namespace_ = $"tenant-{tenantId}";
            var apiServiceUrl = $"http://{tenantId}-api.{namespace_}.svc.cluster.local:15567";

            // Call the tenant API's super-admin login endpoint
            // This endpoint accepts a signed request from the management API
            var client = _httpClientFactory.CreateClient("TenantApi");

            var requestBody = new
            {
                superAdminId = admin.Id,
                superAdminEmail = admin.Email,
                superAdminName = admin.Name,
                tenantId = tenantId,
                timestamp = DateTime.UtcNow.ToString("O"),
                purpose = "troubleshooting"
            };

            // Sign the request with a shared secret (configured in both management and tenant APIs)
            var signature = GenerateRequestSignature(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{apiServiceUrl}/api/auth/super-admin-access")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Add("X-SuperAdmin-Signature", signature);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get tenant access token for {TenantId}: {Error}", tenantId, errorContent);
                return new TenantAccessResult
                {
                    Success = false,
                    Error = $"Failed to authenticate with tenant: {response.StatusCode}"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TenantTokenResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Log the access for audit
            _logger.LogInformation(
                "Super admin {AdminEmail} accessed tenant {TenantId} for troubleshooting",
                admin.Email, tenantId);

            await RecordTenantAccessAsync(admin.Id, tenantId);

            return new TenantAccessResult
            {
                Success = true,
                TenantUrl = tenant.ApiUrl ?? $"https://{tenantId}.mechanicbuddy.app",
                AccessToken = tokenResponse?.Token ?? string.Empty,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // 1-hour session
                TenantId = tenantId,
                TenantName = tenant.CompanyName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tenant access for {TenantId}", tenantId);
            return new TenantAccessResult { Success = false, Error = "Internal error generating access" };
        }
    }

    /// <summary>
    /// Generate a direct access URL with embedded token for super admin to access tenant.
    /// This is an alternative method that generates a one-time access link.
    /// </summary>
    public async Task<TenantAccessResult> GenerateDirectAccessUrlAsync(int superAdminId, string tenantId)
    {
        var admin = await _superAdminRepository.GetByIdAsync(superAdminId);
        if (admin == null || !admin.IsActive)
        {
            return new TenantAccessResult { Success = false, Error = "Super admin not found or inactive" };
        }

        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            return new TenantAccessResult { Success = false, Error = "Tenant not found" };
        }

        // Generate a one-time access token
        var accessToken = GenerateOneTimeAccessToken(admin.Id, tenantId);

        // Store the token for validation (expires in 5 minutes)
        await StoreOneTimeTokenAsync(accessToken, admin.Id, tenantId, TimeSpan.FromMinutes(5));

        var tenantUrl = tenant.ApiUrl ?? $"https://{tenantId}.mechanicbuddy.app";
        var accessUrl = $"{tenantUrl}/auth/super-admin?token={accessToken}";

        _logger.LogInformation(
            "Generated direct access URL for super admin {AdminEmail} to tenant {TenantId}",
            admin.Email, tenantId);

        return new TenantAccessResult
        {
            Success = true,
            TenantUrl = accessUrl,
            AccessToken = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            TenantId = tenantId,
            TenantName = tenant.CompanyName
        };
    }

    /// <summary>
    /// Get audit log of super admin tenant accesses.
    /// </summary>
    public async Task<IEnumerable<TenantAccessLog>> GetAccessLogsAsync(int? superAdminId = null, string? tenantId = null, int limit = 100)
    {
        return await _superAdminRepository.GetAccessLogsAsync(superAdminId, tenantId, limit);
    }

    private string GenerateRequestSignature(object requestBody)
    {
        // In production, use HMAC-SHA256 with a shared secret
        var json = JsonSerializer.Serialize(requestBody);
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SUPER_ADMIN_SHARED_SECRET") ?? "dev-secret"));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(hash);
    }

    private string GenerateOneTimeAccessToken(int adminId, string tenantId)
    {
        var tokenData = $"{adminId}:{tenantId}:{DateTime.UtcNow.Ticks}:{Guid.NewGuid()}";
        var bytes = Encoding.UTF8.GetBytes(tokenData);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private async Task StoreOneTimeTokenAsync(string token, int adminId, string tenantId, TimeSpan expiry)
    {
        // Store in database for validation
        await _superAdminRepository.StoreOneTimeTokenAsync(token, adminId, tenantId, DateTime.UtcNow.Add(expiry));
    }

    private async Task RecordTenantAccessAsync(int adminId, string tenantId)
    {
        await _superAdminRepository.RecordTenantAccessAsync(adminId, tenantId, DateTime.UtcNow);
    }
}

/// <summary>
/// Result of tenant access request.
/// </summary>
public class TenantAccessResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? TenantUrl { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? TenantId { get; set; }
    public string? TenantName { get; set; }
}

/// <summary>
/// Response from tenant API token endpoint.
/// </summary>
public class TenantTokenResponse
{
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Audit log entry for tenant access.
/// </summary>
public class TenantAccessLog
{
    public int Id { get; set; }
    public int SuperAdminId { get; set; }
    public string? SuperAdminEmail { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string? TenantName { get; set; }
    public DateTime AccessedAt { get; set; }
    public string? IpAddress { get; set; }
}
