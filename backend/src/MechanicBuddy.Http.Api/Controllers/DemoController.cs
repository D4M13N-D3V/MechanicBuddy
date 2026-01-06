using System;
using System.Data;
using System.Threading.Tasks;
using Carmasters.Core.Application;
using Carmasters.Core.Application.Authorization;
using Carmasters.Core.Application.Configuration;
using Carmasters.Core.Application.Database;
using Carmasters.Core.Application.Model;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Domain;
using Carmasters.Http.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Carmasters.Http.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly ILogger<DemoController> _logger; 
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly DbOptions _dbOptions;
        private readonly IDemoSetupService _demoSetupService;

        public DemoController(
              ILogger<DemoController> logger,
              IConfiguration configuration,
              IOptions<JwtOptions> jwtOptions,
              IOptions<DbOptions> dbOptions,
              IDemoSetupService demoSetupService)
        {
            _logger = logger; 
            _jwtOptions = jwtOptions;
            _dbOptions = dbOptions.Value;
            _demoSetupService = demoSetupService;
        }

        [AllowAnonymous]
        [DemoRateLimit]
        [HttpPost("setup")]
        public async Task<ActionResult<DemoSetupResponse>> SetupDemo([FromBody] DemoSetupRequest request)
        {
            if (_dbOptions.MultiTenancy?.Enabled != true)
            {
                return BadRequest("Demo setup requires multi-tenancy to be enabled");
            }

            try
            {
                // Use the demo setup service to create a new tenant with sample data
                var (username, password, tenantName) = await _demoSetupService.CreateDemoTenant(request.CompanyName);
                 
                var response = new DemoSetupResponse
                {
                    Username = username,
                    Password = password  
                };

                _logger.LogInformation("Created demo instance with tenant name {TenantName}", tenantName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create demo instance: {Error}", ex.Message);
                return StatusCode(500, "Failed to create demo instance");
            }
        }
    }
}