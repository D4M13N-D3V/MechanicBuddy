using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MechanicBuddy.Http.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public VersionController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Get application version information
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetVersion()
    {
        var version = _configuration["App:Version"] ?? "0.0.0";
        var buildSha = _configuration["App:BuildSha"] ?? "unknown";
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

        return Ok(new
        {
            version,
            buildSha,
            fullVersion = buildSha != "unknown" ? $"{version}-{buildSha}" : version,
            environment,
            timestamp = DateTime.UtcNow
        });
    }
}
