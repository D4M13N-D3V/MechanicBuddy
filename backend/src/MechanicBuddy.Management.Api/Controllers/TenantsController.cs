using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;
using MechanicBuddy.Management.Api.Domain;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly TenantService _tenantService;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(TenantService tenantService, ILogger<TenantsController> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var tenants = await _tenantService.GetAllAsync(skip, take);
        return Ok(tenants);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tenant = await _tenantService.GetByIdAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }
        return Ok(tenant);
    }

    [HttpGet("by-tenant-id/{tenantId}")]
    public async Task<IActionResult> GetByTenantId(string tenantId)
    {
        var tenant = await _tenantService.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            return NotFound();
        }
        return Ok(tenant);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        try
        {
            var tenant = await _tenantService.CreateTenantAsync(
                request.CompanyName,
                request.OwnerEmail,
                request.OwnerName,
                request.Tier ?? "free",
                request.IsDemo
            );

            return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tenant");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Tenant tenant)
    {
        if (id != tenant.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var success = await _tenantService.UpdateTenantAsync(tenant);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{tenantId}/suspend")]
    public async Task<IActionResult> Suspend(string tenantId, [FromBody] SuspendTenantRequest request)
    {
        var success = await _tenantService.SuspendTenantAsync(tenantId, request.Reason);
        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "Tenant suspended successfully" });
    }

    [HttpPost("{tenantId}/resume")]
    public async Task<IActionResult> Resume(string tenantId)
    {
        var success = await _tenantService.ResumeTenantAsync(tenantId);
        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "Tenant resumed successfully" });
    }

    [HttpDelete("{tenantId}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> Delete(string tenantId)
    {
        var success = await _tenantService.DeleteTenantAsync(tenantId);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _tenantService.GetStatsAsync();
        return Ok(stats);
    }
}

public record CreateTenantRequest(string CompanyName, string OwnerEmail, string OwnerName, string? Tier, bool IsDemo);
public record SuspendTenantRequest(string Reason);
