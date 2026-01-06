using System;
using System.Text;
using System.Threading.Tasks;
using Carmasters.Core.Application.Configuration;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Persistence.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Carmasters.Http.Api.Controllers
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class OptionsController : ControllerBase
    {
        private readonly ITenantConfigService tenantConfigService;
        private readonly DatabaseBackup backup;
        private readonly ILogger<OptionsController> logger;

        public OptionsController(
            ITenantConfigService tenantConfigService,
            DatabaseBackup backup,
            ILogger<OptionsController> logger)
        {
            this.tenantConfigService = tenantConfigService;
            this.backup = backup;
            this.logger = logger;
        }

        [HttpGet()]
        public async Task<ActionResult<AppOptions>> Get()
        {
            try
            {
                return await tenantConfigService.GetAppOptionsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving tenant configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve configuration");
            }
        }

        [HttpPut]
        public async Task<ActionResult> Post([FromBody] AppOptions appOptions)
        {
            try
            {
                await tenantConfigService.SaveAppOptionsAsync(appOptions);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving tenant configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save configuration");
            }
        }

        [HttpGet("dbdump")]
        public async Task<IActionResult> DumpDb()
        {
            var script = await backup.Dump();
            Response.Headers.Append("content-disposition", $"inline;filename=dbdump{DateTime.Now.ToString("yyyyMMddmmss")}.sql");

            return File(Encoding.UTF8.GetBytes(script), "application/octet-stream");
        }
    }
}