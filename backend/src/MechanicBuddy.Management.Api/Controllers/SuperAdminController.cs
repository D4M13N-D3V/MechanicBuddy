using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly")]
public class SuperAdminController : ControllerBase
{
    private readonly SuperAdminService _superAdminService;
    private readonly ILogger<SuperAdminController> _logger;

    public SuperAdminController(SuperAdminService superAdminService, ILogger<SuperAdminController> logger)
    {
        _superAdminService = superAdminService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var admins = await _superAdminService.GetAllAsync();
        // Don't return password hashes
        var sanitizedAdmins = admins.Select(a => new
        {
            a.Id,
            a.Email,
            a.Name,
            a.Role,
            a.IsActive,
            a.CreatedAt,
            a.LastLoginAt
        });
        return Ok(sanitizedAdmins);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var admin = await _superAdminService.GetByIdAsync(id);
        if (admin == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            admin.Id,
            admin.Email,
            admin.Name,
            admin.Role,
            admin.IsActive,
            admin.CreatedAt,
            admin.LastLoginAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminRequest request)
    {
        try
        {
            var admin = await _superAdminService.CreateAdminAsync(
                request.Email,
                request.Password,
                request.Name,
                request.Role ?? "admin"
            );

            return CreatedAtAction(nameof(GetById), new { id = admin.Id }, new
            {
                admin.Id,
                admin.Email,
                admin.Name,
                admin.Role,
                admin.IsActive,
                admin.CreatedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var success = await _superAdminService.DeactivateAdminAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "Admin deactivated successfully" });
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        var success = await _superAdminService.ActivateAdminAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "Admin activated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _superAdminService.DeleteAdminAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Generate access token to enter a tenant's dashboard for troubleshooting.
    /// </summary>
    [HttpPost("tenant-access/{tenantId}")]
    public async Task<IActionResult> GenerateTenantAccess(string tenantId)
    {
        var adminId = GetCurrentAdminId();
        if (adminId == null)
        {
            return Unauthorized();
        }

        var result = await _superAdminService.GenerateTenantAccessAsync(adminId.Value, tenantId);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new
        {
            tenantUrl = result.TenantUrl,
            accessToken = result.AccessToken,
            expiresAt = result.ExpiresAt,
            tenantId = result.TenantId,
            tenantName = result.TenantName
        });
    }

    /// <summary>
    /// Generate a one-time direct access URL for a tenant.
    /// </summary>
    [HttpPost("tenant-access/{tenantId}/direct")]
    public async Task<IActionResult> GenerateDirectAccess(string tenantId)
    {
        var adminId = GetCurrentAdminId();
        if (adminId == null)
        {
            return Unauthorized();
        }

        var result = await _superAdminService.GenerateDirectAccessUrlAsync(adminId.Value, tenantId);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new
        {
            accessUrl = result.TenantUrl,
            expiresAt = result.ExpiresAt,
            tenantId = result.TenantId,
            tenantName = result.TenantName
        });
    }

    /// <summary>
    /// Get audit log of tenant accesses.
    /// </summary>
    [HttpGet("access-logs")]
    public async Task<IActionResult> GetAccessLogs([FromQuery] int? adminId, [FromQuery] string? tenantId, [FromQuery] int limit = 100)
    {
        var logs = await _superAdminService.GetAccessLogsAsync(adminId, tenantId, limit);
        return Ok(logs);
    }

    private int? GetCurrentAdminId()
    {
        var claim = User.FindFirst("admin_id") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out var id))
        {
            return id;
        }
        return null;
    }
}

public record CreateAdminRequest(string Email, string Password, string Name, string? Role);
