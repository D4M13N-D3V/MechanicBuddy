using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;
using System.Security.Claims;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly TenantService _tenantService;
    private readonly SuperAdminService _adminService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        TenantService tenantService,
        SuperAdminService adminService,
        ILogger<UserController> logger)
    {
        _tenantService = tenantService;
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current logged-in user's profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new { message = "User email not found in token" });
        }

        var user = await _adminService.GetByEmailAsync(email);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Name,
            user.Role,
            user.CreatedAt
        });
    }

    /// <summary>
    /// Get tenants owned by the current user
    /// </summary>
    [HttpGet("tenants")]
    public async Task<IActionResult> GetMyTenants()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new { message = "User email not found in token" });
        }

        _logger.LogInformation("Getting tenants for user: {Email}", email);

        var tenants = await _tenantService.GetTenantsByOwnerEmailAsync(email);

        return Ok(tenants);
    }

    /// <summary>
    /// Request a new tenant for the current user
    /// </summary>
    [HttpPost("request-tenant")]
    public async Task<IActionResult> RequestNewTenant([FromBody] RequestTenantRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value ?? "User";

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new { message = "User email not found in token" });
        }

        try
        {
            var tenant = await _tenantService.CreateTenantAsync(
                request.CompanyName,
                email,
                name,
                "free",
                false
            );

            _logger.LogInformation(
                "Created new tenant {TenantId} for user {Email}",
                tenant.TenantId,
                email
            );

            return Ok(tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tenant for user {Email}", email);
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record RequestTenantRequest(string CompanyName, string? Message);
