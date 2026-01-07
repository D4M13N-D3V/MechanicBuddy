using System;
using System.Threading.Tasks;
using MechanicBuddy.Core.Application.Configuration;
using MechanicBuddy.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MechanicBuddy.Http.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class PublicLandingController : ControllerBase
    {
        private readonly IBrandingService brandingService;
        private readonly ILogger<PublicLandingController> logger;

        public PublicLandingController(
            IBrandingService brandingService,
            ILogger<PublicLandingController> logger)
        {
            this.brandingService = brandingService;
            this.logger = logger;
        }

        // GET /api/publiclanding
        [HttpGet]
        [ResponseCache(Duration = 60)] // Cache for 1 minute
        public async Task<ActionResult<PublicLandingData>> GetLandingData()
        {
            try
            {
                return await brandingService.GetPublicLandingDataAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving public landing data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve landing data");
            }
        }

        // GET /api/publiclanding/branding
        [HttpGet("branding")]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<BrandingOptions>> GetBranding()
        {
            try
            {
                return await brandingService.GetBrandingAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving public branding");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve branding");
            }
        }

        // GET /api/publiclanding/content
        [HttpGet("content")]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<LandingContentOptions>> GetContent()
        {
            try
            {
                return await brandingService.GetLandingContentAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving public landing content");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve landing content");
            }
        }
    }
}
