using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;
using MechanicBuddy.Management.Api.Authorization;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SuperAdminService _superAdminService;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        SuperAdminService superAdminService,
        JwtService jwtService,
        ILogger<AuthController> logger)
    {
        _superAdminService = superAdminService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var admin = await _superAdminService.AuthenticateAsync(request.Email, request.Password);
        if (admin == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = _jwtService.GenerateToken(admin);

        return Ok(new
        {
            token,
            admin = new
            {
                admin.Id,
                admin.Email,
                admin.Name,
                admin.Role
            }
        });
    }

    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out var adminId))
        {
            return Unauthorized();
        }

        var success = await _superAdminService.UpdatePasswordAsync(adminId, request.CurrentPassword, request.NewPassword);
        if (!success)
        {
            return BadRequest(new { message = "Failed to update password. Current password may be incorrect." });
        }

        return Ok(new { message = "Password updated successfully" });
    }
}

public record LoginRequest(string Email, string Password);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
