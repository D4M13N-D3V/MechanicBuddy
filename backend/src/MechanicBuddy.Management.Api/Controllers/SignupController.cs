using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;
using MechanicBuddy.Management.Api.Authorization;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SignupController : ControllerBase
{
    private readonly SuperAdminService _superAdminService;
    private readonly TenantService _tenantService;
    private readonly JwtService _jwtService;
    private readonly ILogger<SignupController> _logger;

    public SignupController(
        SuperAdminService superAdminService,
        TenantService tenantService,
        JwtService jwtService,
        ILogger<SignupController> logger)
    {
        _superAdminService = superAdminService;
        _tenantService = tenantService;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Register new user account and auto-provision free tenant
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.CompanyName))
            {
                return BadRequest(new { message = "All fields are required" });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new { message = "Password must be at least 8 characters long" });
            }

            // Check if email already exists
            var existingAdmin = await _superAdminService.GetByEmailAsync(request.Email);
            if (existingAdmin != null)
            {
                return Conflict(new { message = "An account with this email already exists" });
            }

            // Create user account with "user" role (not super_admin)
            var admin = await _superAdminService.CreateAdminAsync(
                request.Email,
                request.Password,
                request.Name,
                role: "user"
            );

            _logger.LogInformation("Created new user account: {Email}", request.Email);

            // Auto-provision a free tier tenant
            var tenant = await _tenantService.CreateTenantAsync(
                companyName: request.CompanyName,
                ownerEmail: request.Email,
                ownerName: request.Name,
                tier: "free",
                isDemo: false
            );

            _logger.LogInformation(
                "Auto-provisioned free tenant {TenantId} for user {Email}",
                tenant.TenantId,
                request.Email
            );

            // Generate JWT token for auto-login
            var token = _jwtService.GenerateToken(admin);

            return Ok(new
            {
                token,
                user = new
                {
                    admin.Id,
                    admin.Email,
                    admin.Name,
                    admin.Role
                },
                tenant = new
                {
                    tenant.Id,
                    tenant.TenantId,
                    tenant.CompanyName,
                    tenant.ApiUrl,
                    tenant.Status
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Signup failed for {Email}", request.Email);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during signup. Please try again later." });
        }
    }
}

public record SignupRequest(
    string Email,
    string Password,
    string Name,
    string CompanyName
);
