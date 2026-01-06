using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DomainsController : ControllerBase
{
    private readonly DomainService _domainService;
    private readonly ILogger<DomainsController> _logger;

    public DomainsController(DomainService domainService, ILogger<DomainsController> logger)
    {
        _domainService = domainService;
        _logger = logger;
    }

    [HttpPost("verify/initiate")]
    public async Task<IActionResult> InitiateVerification([FromBody] InitiateDomainVerificationRequest request)
    {
        try
        {
            var verification = await _domainService.InitiateDomainVerificationAsync(
                request.TenantId,
                request.Domain,
                request.Method ?? "dns"
            );

            return Ok(new
            {
                verification.Id,
                verification.Domain,
                verification.VerificationToken,
                verification.VerificationMethod,
                verification.ExpiresAt,
                instructions = GetVerificationInstructions(verification.VerificationMethod, verification.Domain, verification.VerificationToken)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate domain verification");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify/{domain}")]
    public async Task<IActionResult> VerifyDomain(string domain)
    {
        var success = await _domainService.VerifyDomainAsync(domain);
        if (!success)
        {
            return BadRequest(new { message = "Domain verification failed" });
        }

        return Ok(new { message = "Domain verified successfully" });
    }

    [HttpGet("status/{domain}")]
    public async Task<IActionResult> GetVerificationStatus(string domain)
    {
        var verification = await _domainService.GetVerificationStatusAsync(domain);
        if (verification == null)
        {
            return NotFound();
        }

        return Ok(verification);
    }

    [HttpDelete("{tenantId}")]
    public async Task<IActionResult> RemoveDomain(int tenantId)
    {
        var success = await _domainService.RemoveDomainAsync(tenantId);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static object GetVerificationInstructions(string method, string domain, string token)
    {
        return method switch
        {
            "dns" => new
            {
                type = "DNS TXT Record",
                host = $"_mechanicbuddy-verification.{domain}",
                value = $"mechanicbuddy-verification={token}",
                description = "Add this TXT record to your DNS settings"
            },
            "file" => new
            {
                type = "File Upload",
                path = $"https://{domain}/.well-known/mechanicbuddy-verification.txt",
                content = token,
                description = "Upload a file with this content to the specified path"
            },
            _ => new { }
        };
    }
}

public record InitiateDomainVerificationRequest(int TenantId, string Domain, string? Method);
