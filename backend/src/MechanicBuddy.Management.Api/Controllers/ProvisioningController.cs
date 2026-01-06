using MechanicBuddy.Management.Api.Models;
using MechanicBuddy.Management.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicBuddy.Management.Api.Controllers;

/// <summary>
/// Controller for Kubernetes-based tenant provisioning operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly")]
public class ProvisioningController : ControllerBase
{
    private readonly ITenantProvisioningService _provisioningService;
    private readonly ILogger<ProvisioningController> _logger;

    public ProvisioningController(
        ITenantProvisioningService provisioningService,
        ILogger<ProvisioningController> logger)
    {
        _provisioningService = provisioningService;
        _logger = logger;
    }

    /// <summary>
    /// Provisions a new tenant with Kubernetes infrastructure.
    /// </summary>
    /// <param name="request">Tenant provisioning request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Provisioning result with tenant details.</returns>
    [HttpPost("provision")]
    [ProducesResponseType(typeof(TenantProvisioningResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TenantProvisioningResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TenantProvisioningResult>> ProvisionTenant(
        [FromBody] TenantProvisioningRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Received request to provision tenant for company: {CompanyName}",
                request.CompanyName);

            var result = await _provisioningService.ProvisionTenantAsync(request, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Successfully provisioned tenant {TenantId}", result.TenantId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Failed to provision tenant: {Error}", result.ErrorMessage);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error provisioning tenant");
            return StatusCode(500, new TenantProvisioningResult
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred during provisioning"
            });
        }
    }

    /// <summary>
    /// Deprovisions a tenant and removes all Kubernetes resources.
    /// </summary>
    /// <param name="tenantId">Tenant ID to deprovision.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    [HttpPost("{tenantId}/deprovision")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeprovisionTenant(
        string tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Received request to deprovision tenant {TenantId}", tenantId);

            var success = await _provisioningService.DeprovisionTenantAsync(tenantId, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Successfully deprovisioned tenant {TenantId}", tenantId);
                return Ok(new { message = "Tenant deprovisioned successfully", tenantId });
            }
            else
            {
                _logger.LogWarning("Failed to deprovision tenant {TenantId}", tenantId);
                return NotFound(new { message = "Tenant not found or deprovisioning failed", tenantId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deprovisioning tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Updates a tenant's Kubernetes deployment (e.g., scaling, tier upgrade).
    /// </summary>
    /// <param name="tenantId">Tenant ID to update.</param>
    /// <param name="request">Update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Update result.</returns>
    [HttpPut("{tenantId}")]
    [ProducesResponseType(typeof(TenantProvisioningResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TenantProvisioningResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TenantProvisioningResult>> UpdateTenant(
        string tenantId,
        [FromBody] TenantProvisioningRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Received request to update tenant {TenantId}", tenantId);

            var result = await _provisioningService.UpdateTenantAsync(tenantId, request, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Successfully updated tenant {TenantId}", tenantId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Failed to update tenant {TenantId}: {Error}",
                    tenantId, result.ErrorMessage);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating tenant {TenantId}", tenantId);
            return StatusCode(500, new TenantProvisioningResult
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred"
            });
        }
    }

    /// <summary>
    /// Gets the current Kubernetes deployment status of a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tenant status information.</returns>
    [HttpGet("{tenantId}/status")]
    [ProducesResponseType(typeof(TenantStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TenantStatus>> GetTenantStatus(
        string tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var status = await _provisioningService.GetTenantStatusAsync(tenantId, cancellationToken);

            if (status.Status == "NotFound")
            {
                return NotFound(new { message = "Tenant not found", tenantId });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting tenant status for {TenantId}", tenantId);
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Validates a provisioning request without actually provisioning.
    /// </summary>
    /// <param name="request">Request to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ValidationResult>> ValidateRequest(
        [FromBody] TenantProvisioningRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _provisioningService.ValidateProvisioningRequestAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating request");
            return StatusCode(500, new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { "An unexpected error occurred during validation" }
            });
        }
    }

    /// <summary>
    /// Generates a tenant ID from a company name.
    /// </summary>
    /// <param name="companyName">Company name.</param>
    /// <returns>Generated tenant ID.</returns>
    [HttpGet("generate-id")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<object> GenerateTenantId([FromQuery] string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            return BadRequest(new { message = "Company name is required" });
        }

        var tenantId = _provisioningService.GenerateTenantId(companyName);
        return Ok(new { companyName, tenantId });
    }
}
