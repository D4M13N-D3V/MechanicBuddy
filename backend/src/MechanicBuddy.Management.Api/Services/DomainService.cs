using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace MechanicBuddy.Management.Api.Services;

public class DomainService
{
    private readonly IDomainVerificationRepository _domainVerificationRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<DomainService> _logger;

    public DomainService(
        IDomainVerificationRepository domainVerificationRepository,
        ITenantRepository tenantRepository,
        ILogger<DomainService> logger)
    {
        _domainVerificationRepository = domainVerificationRepository;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<DomainVerification> InitiateDomainVerificationAsync(int tenantId, string domain, string method = "dns")
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant not found");
        }

        // Check if domain is already in use
        var existingTenant = await _tenantRepository.GetByCustomDomainAsync(domain);
        if (existingTenant != null && existingTenant.Id != tenantId)
        {
            throw new InvalidOperationException("Domain is already in use by another tenant");
        }

        // Generate verification token
        var token = GenerateVerificationToken();

        var verification = new DomainVerification
        {
            TenantId = tenantId,
            Domain = domain,
            VerificationToken = token,
            VerificationMethod = method,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var id = await _domainVerificationRepository.CreateAsync(verification);
        verification.Id = id;

        _logger.LogInformation("Initiated domain verification for {Domain} (tenant {TenantId})", domain, tenantId);

        return verification;
    }

    public async Task<bool> VerifyDomainAsync(string domain)
    {
        var verification = await _domainVerificationRepository.GetByDomainAsync(domain);
        if (verification == null || verification.IsVerified)
        {
            return false;
        }

        if (verification.ExpiresAt.HasValue && verification.ExpiresAt.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Domain verification for {Domain} has expired", domain);
            return false;
        }

        bool isVerified = verification.VerificationMethod switch
        {
            "dns" => await VerifyDnsTxtRecordAsync(domain, verification.VerificationToken),
            "file" => await VerifyFileAsync(domain, verification.VerificationToken),
            _ => false
        };

        if (isVerified)
        {
            verification.IsVerified = true;
            verification.VerifiedAt = DateTime.UtcNow;
            await _domainVerificationRepository.UpdateAsync(verification);

            // Update tenant
            var tenant = await _tenantRepository.GetByIdAsync(verification.TenantId);
            if (tenant != null)
            {
                tenant.CustomDomain = domain;
                tenant.DomainVerified = true;
                await _tenantRepository.UpdateAsync(tenant);
            }

            _logger.LogInformation("Successfully verified domain {Domain} for tenant {TenantId}", domain, verification.TenantId);
        }

        return isVerified;
    }

    public async Task<DomainVerification?> GetVerificationStatusAsync(string domain)
    {
        return await _domainVerificationRepository.GetByDomainAsync(domain);
    }

    public async Task<bool> RemoveDomainAsync(int tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            return false;
        }

        tenant.CustomDomain = null;
        tenant.DomainVerified = false;
        return await _tenantRepository.UpdateAsync(tenant);
    }

    private static string GenerateVerificationToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes).Replace("+", "").Replace("/", "").Replace("=", "")[..32];
    }

    private async Task<bool> VerifyDnsTxtRecordAsync(string domain, string expectedToken)
    {
        try
        {
            // In a real implementation, you would use a DNS resolver library
            // For now, this is a placeholder
            _logger.LogInformation("Checking DNS TXT record for {Domain}", domain);

            // Example: dig TXT _mechanicbuddy-verification.{domain}
            // Should return: mechanicbuddy-verification={expectedToken}

            // Placeholder - implement actual DNS verification
            await Task.Delay(100);

            return false; // Implement actual DNS lookup
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify DNS record for {Domain}", domain);
            return false;
        }
    }

    private async Task<bool> VerifyFileAsync(string domain, string expectedToken)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var url = $"https://{domain}/.well-known/mechanicbuddy-verification.txt";

            var response = await httpClient.GetStringAsync(url);
            var actualToken = response.Trim();

            return actualToken == expectedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify file for {Domain}", domain);
            return false;
        }
    }
}
