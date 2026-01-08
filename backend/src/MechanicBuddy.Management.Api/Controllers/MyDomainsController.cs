using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;
using MechanicBuddy.Management.Api.Repositories;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace MechanicBuddy.Management.Api.Controllers;

/// <summary>
/// Controller for tenant owners to manage their custom domains
/// </summary>
[ApiController]
[Route("api/user/tenants/{tenantId}/domains")]
[Authorize]
public class MyDomainsController : ControllerBase
{
    private readonly DomainService _domainService;
    private readonly TenantService _tenantService;
    private readonly IDomainVerificationRepository _domainVerificationRepository;
    private readonly ILogger<MyDomainsController> _logger;

    // Domain validation regex - allows subdomains and apex domains
    private static readonly Regex DomainRegex = new(
        @"^(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public MyDomainsController(
        DomainService domainService,
        TenantService tenantService,
        IDomainVerificationRepository domainVerificationRepository,
        ILogger<MyDomainsController> logger)
    {
        _domainService = domainService;
        _tenantService = tenantService;
        _domainVerificationRepository = domainVerificationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Initiate domain verification for a tenant
    /// POST /api/user/tenants/{tenantId}/domains
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> InitiateDomainVerification(int tenantId, [FromBody] AddDomainRequest request)
    {
        var authResult = await AuthorizeTenantOwnerAsync(tenantId);
        if (authResult != null) return authResult;

        if (string.IsNullOrWhiteSpace(request.Domain))
        {
            return BadRequest(new { message = "Domain is required", errorCode = "DOMAIN_REQUIRED" });
        }

        // Normalize and validate domain
        var domain = NormalizeDomain(request.Domain);
        var validationError = ValidateDomain(domain);
        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        try
        {
            var verification = await _domainService.InitiateDomainVerificationAsync(
                tenantId,
                domain,
                "dns"
            );

            _logger.LogInformation(
                "Domain verification initiated for {Domain} by tenant owner (tenant {TenantId})",
                domain, tenantId);

            return Ok(new
            {
                verification.Id,
                verification.Domain,
                verification.VerificationToken,
                verification.VerificationMethod,
                verification.ExpiresAt,
                verification.IsVerified,
                instructions = GetVerificationInstructions(verification.Domain, verification.VerificationToken)
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid domain operation for tenant {TenantId}", tenantId);

            var errorCode = ex.Message.Contains("already in use") ? "DOMAIN_IN_USE" : "INVALID_OPERATION";
            return BadRequest(new { message = ex.Message, errorCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate domain verification for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Internal server error", errorCode = "INTERNAL_ERROR" });
        }
    }

    /// <summary>
    /// List all domains for a tenant
    /// GET /api/user/tenants/{tenantId}/domains
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListDomains(int tenantId)
    {
        var authResult = await AuthorizeTenantOwnerAsync(tenantId);
        if (authResult != null) return authResult;

        try
        {
            var verifications = await _domainVerificationRepository.GetByTenantIdAsync(tenantId);

            var domains = verifications.Select(v => new
            {
                v.Id,
                v.Domain,
                v.IsVerified,
                v.VerificationMethod,
                v.VerificationToken,
                v.CreatedAt,
                v.VerifiedAt,
                v.ExpiresAt,
                instructions = v.IsVerified ? null : GetVerificationInstructions(v.Domain, v.VerificationToken)
            });

            return Ok(new { domains });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list domains for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Internal server error", errorCode = "INTERNAL_ERROR" });
        }
    }

    /// <summary>
    /// Verify a domain (check DNS records)
    /// POST /api/user/tenants/{tenantId}/domains/{domain}/verify
    /// </summary>
    [HttpPost("{domain}/verify")]
    public async Task<IActionResult> VerifyDomain(int tenantId, string domain)
    {
        var authResult = await AuthorizeTenantOwnerAsync(tenantId);
        if (authResult != null) return authResult;

        try
        {
            var decodedDomain = Uri.UnescapeDataString(domain);

            // Get verification status first
            var verification = await _domainService.GetVerificationStatusAsync(decodedDomain);
            if (verification == null || verification.TenantId != tenantId)
            {
                return NotFound(new
                {
                    success = false,
                    status = "not_found",
                    errorCode = "DOMAIN_NOT_FOUND",
                    errorMessage = "Domain verification not found for this tenant"
                });
            }

            if (verification.IsVerified)
            {
                return Ok(new
                {
                    success = true,
                    status = "verified",
                    domain = decodedDomain,
                    verifiedAt = verification.VerifiedAt
                });
            }

            // Check if expired
            if (verification.ExpiresAt.HasValue && verification.ExpiresAt.Value < DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    success = false,
                    status = "expired",
                    errorCode = "VERIFICATION_EXPIRED",
                    errorMessage = "Verification token has expired. Please remove this domain and add it again to get a new token."
                });
            }

            // Attempt verification with detailed results
            var result = await _domainService.VerifyDomainWithDetailsAsync(decodedDomain);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Domain {Domain} verified successfully for tenant {TenantId}",
                    decodedDomain, tenantId);

                return Ok(new
                {
                    success = true,
                    status = "verified",
                    domain = decodedDomain,
                    verifiedAt = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                success = false,
                status = "pending",
                errorCode = result.ErrorCode,
                errorMessage = result.ErrorMessage,
                dnsCheck = result.DnsCheck
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify domain {Domain} for tenant {TenantId}", domain, tenantId);
            return StatusCode(500, new
            {
                success = false,
                status = "error",
                errorCode = "INTERNAL_ERROR",
                errorMessage = "An error occurred while checking the domain"
            });
        }
    }

    /// <summary>
    /// Get domain verification status
    /// GET /api/user/tenants/{tenantId}/domains/{domain}/status
    /// </summary>
    [HttpGet("{domain}/status")]
    public async Task<IActionResult> GetDomainStatus(int tenantId, string domain)
    {
        var authResult = await AuthorizeTenantOwnerAsync(tenantId);
        if (authResult != null) return authResult;

        try
        {
            var decodedDomain = Uri.UnescapeDataString(domain);
            var verification = await _domainService.GetVerificationStatusAsync(decodedDomain);

            if (verification == null || verification.TenantId != tenantId)
            {
                return NotFound(new { message = "Domain not found for this tenant", errorCode = "DOMAIN_NOT_FOUND" });
            }

            var isExpired = verification.ExpiresAt.HasValue &&
                           verification.ExpiresAt.Value < DateTime.UtcNow &&
                           !verification.IsVerified;

            return Ok(new
            {
                verification.Id,
                verification.Domain,
                verification.IsVerified,
                verification.VerificationMethod,
                verification.VerificationToken,
                verification.CreatedAt,
                verification.VerifiedAt,
                verification.ExpiresAt,
                isExpired,
                instructions = verification.IsVerified ? null : GetVerificationInstructions(
                    verification.Domain,
                    verification.VerificationToken)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for domain {Domain}", domain);
            return StatusCode(500, new { message = "Internal server error", errorCode = "INTERNAL_ERROR" });
        }
    }

    /// <summary>
    /// Remove a custom domain from a tenant
    /// DELETE /api/user/tenants/{tenantId}/domains/{domain}
    /// </summary>
    [HttpDelete("{domain}")]
    public async Task<IActionResult> RemoveDomain(int tenantId, string domain)
    {
        var authResult = await AuthorizeTenantOwnerAsync(tenantId);
        if (authResult != null) return authResult;

        try
        {
            var decodedDomain = Uri.UnescapeDataString(domain);

            // Get the verification record for this domain
            var verification = await _domainVerificationRepository.GetByDomainAsync(decodedDomain);
            if (verification == null || verification.TenantId != tenantId)
            {
                return NotFound(new { message = "Domain not found for this tenant", errorCode = "DOMAIN_NOT_FOUND" });
            }

            // Remove from database
            await _domainVerificationRepository.DeleteAsync(verification.Id);

            // Remove from tenant (clears CustomDomain field and updates ingress)
            await _domainService.RemoveDomainAsync(tenantId);

            _logger.LogInformation(
                "Domain {Domain} removed by tenant owner (tenant {TenantId})",
                decodedDomain, tenantId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove domain {Domain} for tenant {TenantId}", domain, tenantId);
            return StatusCode(500, new { message = "Internal server error", errorCode = "INTERNAL_ERROR" });
        }
    }

    /// <summary>
    /// Authorize that the current user owns the specified tenant
    /// </summary>
    private async Task<IActionResult?> AuthorizeTenantOwnerAsync(int tenantId)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new { message = "User email not found in token", errorCode = "UNAUTHORIZED" });
        }

        var tenant = await _tenantService.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            return NotFound(new { message = "Tenant not found", errorCode = "TENANT_NOT_FOUND" });
        }

        // Check if user owns this tenant
        if (!string.Equals(tenant.OwnerEmail, email, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "User {Email} attempted to access tenant {TenantId} without ownership",
                email, tenantId);
            return Forbid();
        }

        return null;
    }

    /// <summary>
    /// Normalize domain input (lowercase, trim, remove protocol)
    /// </summary>
    private static string NormalizeDomain(string domain)
    {
        var normalized = domain.Trim().ToLowerInvariant();

        // Remove protocol if present
        if (normalized.StartsWith("https://"))
            normalized = normalized[8..];
        else if (normalized.StartsWith("http://"))
            normalized = normalized[7..];

        // Remove trailing slash
        normalized = normalized.TrimEnd('/');

        // Remove www. prefix (optional)
        // if (normalized.StartsWith("www."))
        //     normalized = normalized[4..];

        return normalized;
    }

    /// <summary>
    /// Validate domain format
    /// </summary>
    private static object? ValidateDomain(string domain)
    {
        if (!DomainRegex.IsMatch(domain))
        {
            return new
            {
                message = "Invalid domain format. Please enter a valid domain like 'workshop.example.com'",
                errorCode = "INVALID_DOMAIN_FORMAT"
            };
        }

        // Block reserved domains
        if (domain.EndsWith(".mechanicbuddy.app", StringComparison.OrdinalIgnoreCase))
        {
            return new
            {
                message = "Cannot use mechanicbuddy.app subdomains as custom domains",
                errorCode = "RESERVED_DOMAIN"
            };
        }

        // Block common test/local domains
        var blockedPatterns = new[] { "localhost", "example.com", "test.com", "invalid" };
        if (blockedPatterns.Any(p => domain.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            return new
            {
                message = "This domain cannot be used",
                errorCode = "BLOCKED_DOMAIN"
            };
        }

        return null;
    }

    /// <summary>
    /// Get verification instructions for DNS TXT record
    /// </summary>
    private static object GetVerificationInstructions(string domain, string token)
    {
        return new
        {
            type = "DNS TXT Record",
            host = $"_mechanicbuddy-verify.{domain}",
            value = token,
            alternativeHost = $"_mechanicbuddy-verify",
            description = "Add a TXT record to your domain's DNS settings with the host and value shown above. DNS changes can take up to 48 hours to propagate, though most changes take effect within a few minutes."
        };
    }
}
