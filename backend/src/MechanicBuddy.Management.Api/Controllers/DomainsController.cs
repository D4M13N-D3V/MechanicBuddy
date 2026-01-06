using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;
using MechanicBuddy.Management.Api.Repositories;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/tenants/{tenantId}/domains")]
[Authorize]
public class DomainsController : ControllerBase
{
    private readonly DomainService _domainService;
    private readonly IDomainVerificationRepository _domainVerificationRepository;
    private readonly ILogger<DomainsController> _logger;

    public DomainsController(
        DomainService domainService,
        IDomainVerificationRepository domainVerificationRepository,
        ILogger<DomainsController> logger)
    {
        _domainService = domainService;
        _domainVerificationRepository = domainVerificationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Add a custom domain to a tenant (initiates verification process)
    /// POST /api/tenants/{tenantId}/domains
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddCustomDomain(int tenantId, [FromBody] AddDomainRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Domain))
            {
                return BadRequest(new { message = "Domain is required" });
            }

            var verification = await _domainService.InitiateDomainVerificationAsync(
                tenantId,
                request.Domain,
                request.VerificationMethod ?? "dns"
            );

            return Ok(new
            {
                verification.Id,
                verification.Domain,
                verification.VerificationToken,
                verification.VerificationMethod,
                verification.ExpiresAt,
                instructions = GetVerificationInstructions(
                    verification.VerificationMethod,
                    verification.Domain,
                    verification.VerificationToken)
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid domain operation for tenant {TenantId}", tenantId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add custom domain for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// List all domains for a tenant
    /// GET /api/tenants/{tenantId}/domains
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListDomains(int tenantId)
    {
        try
        {
            var verifications = await _domainVerificationRepository.GetByTenantIdAsync(tenantId);

            var domains = verifications.Select(v => new
            {
                v.Id,
                v.Domain,
                v.IsVerified,
                v.VerificationMethod,
                v.CreatedAt,
                v.VerifiedAt,
                v.ExpiresAt
            });

            return Ok(new { domains });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list domains for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Trigger domain verification
    /// POST /api/tenants/{tenantId}/domains/{domain}/verify
    /// </summary>
    [HttpPost("{domain}/verify")]
    public async Task<IActionResult> VerifyDomain(int tenantId, string domain)
    {
        try
        {
            var success = await _domainService.VerifyDomainAsync(domain);
            if (!success)
            {
                return BadRequest(new
                {
                    message = "Domain verification failed. Please ensure DNS records are properly configured and try again."
                });
            }

            return Ok(new
            {
                message = "Domain verified successfully",
                domain,
                verifiedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify domain {Domain} for tenant {TenantId}", domain, tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Remove a custom domain from a tenant
    /// DELETE /api/tenants/{tenantId}/domains/{domain}
    /// </summary>
    [HttpDelete("{domain}")]
    public async Task<IActionResult> RemoveDomain(int tenantId, string domain)
    {
        try
        {
            // Get the verification record for this domain
            var verification = await _domainVerificationRepository.GetByDomainAsync(domain);
            if (verification == null || verification.TenantId != tenantId)
            {
                return NotFound(new { message = "Domain not found for this tenant" });
            }

            // Remove from database
            await _domainVerificationRepository.DeleteAsync(verification.Id);

            // Remove from tenant
            var success = await _domainService.RemoveDomainAsync(tenantId);
            if (!success)
            {
                _logger.LogWarning("Failed to remove domain from tenant {TenantId}", tenantId);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove domain {Domain} for tenant {TenantId}", domain, tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get domain verification status
    /// GET /api/tenants/{tenantId}/domains/{domain}/status
    /// </summary>
    [HttpGet("{domain}/status")]
    public async Task<IActionResult> GetDomainStatus(int tenantId, string domain)
    {
        try
        {
            var verification = await _domainService.GetVerificationStatusAsync(domain);
            if (verification == null || verification.TenantId != tenantId)
            {
                return NotFound(new { message = "Domain not found for this tenant" });
            }

            return Ok(new
            {
                verification.Domain,
                verification.IsVerified,
                verification.VerificationMethod,
                verification.VerificationToken,
                verification.CreatedAt,
                verification.VerifiedAt,
                verification.ExpiresAt,
                instructions = verification.IsVerified
                    ? null
                    : GetVerificationInstructions(
                        verification.VerificationMethod,
                        verification.Domain,
                        verification.VerificationToken)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for domain {Domain}", domain);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    private static object GetVerificationInstructions(string method, string domain, string token)
    {
        return method switch
        {
            "dns" => new
            {
                type = "DNS TXT Record",
                host = $"_mechanicbuddy-verify.{domain}",
                value = token,
                alternativeValue = $"mechanicbuddy-verification={token}",
                description = "Add a TXT record to your DNS with the host '_mechanicbuddy-verify' and the value shown above"
            },
            "file" => new
            {
                type = "File Upload",
                path = $"https://{domain}/.well-known/mechanicbuddy-verification.txt",
                content = token,
                description = "Upload a file with this content to the specified path on your domain"
            },
            _ => new { }
        };
    }
}

public record AddDomainRequest(string Domain, string? VerificationMethod);
