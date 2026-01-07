using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MechanicBuddy.Core.Application.Configuration;
using MechanicBuddy.Core.Application.RateLimiting;
using MechanicBuddy.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MechanicBuddy.Http.Api.Controllers
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class BrandingController : ControllerBase
    {
        private readonly IBrandingService brandingService;
        private readonly ILogger<BrandingController> logger;

        public BrandingController(
            IBrandingService brandingService,
            ILogger<BrandingController> logger)
        {
            this.brandingService = brandingService;
            this.logger = logger;
        }

        // GET /api/branding
        [HttpGet]
        public async Task<ActionResult<BrandingOptions>> GetBranding()
        {
            try
            {
                return await brandingService.GetBrandingAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving branding");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve branding");
            }
        }

        // PUT /api/branding
        [HttpPut]
        public async Task<ActionResult> UpdateBranding([FromBody] BrandingOptions options)
        {
            try
            {
                await brandingService.SaveBrandingAsync(options);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving branding");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save branding");
            }
        }

        // GET /api/branding/logo (public endpoint)
        [AllowAnonymous]
        [HttpGet("logo")]
        public async Task<IActionResult> GetLogo()
        {
            try
            {
                var branding = await brandingService.GetBrandingAsync();
                if (string.IsNullOrEmpty(branding.LogoBase64))
                {
                    return NotFound();
                }

                var bytes = Convert.FromBase64String(branding.LogoBase64);
                return File(bytes, branding.LogoMimeType ?? "image/png");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving logo");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve logo");
            }
        }

        // GET /api/branding/hero
        [HttpGet("hero")]
        public async Task<ActionResult<HeroOptions>> GetHero()
        {
            try
            {
                return await brandingService.GetHeroAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving hero");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve hero");
            }
        }

        // PUT /api/branding/hero
        [HttpPut("hero")]
        public async Task<ActionResult> UpdateHero([FromBody] HeroOptions options)
        {
            try
            {
                await brandingService.SaveHeroAsync(options);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving hero");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save hero");
            }
        }

        // GET /api/branding/hero/background (public endpoint)
        [AllowAnonymous]
        [HttpGet("hero/background")]
        public async Task<IActionResult> GetHeroBackground()
        {
            try
            {
                var hero = await brandingService.GetHeroAsync();
                if (string.IsNullOrEmpty(hero.BackgroundImageBase64))
                {
                    return NotFound();
                }

                var bytes = Convert.FromBase64String(hero.BackgroundImageBase64);
                return File(bytes, hero.BackgroundImageMimeType ?? "image/jpeg");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving hero background");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve hero background");
            }
        }

        // === Services CRUD ===

        // GET /api/branding/services
        [HttpGet("services")]
        public async Task<ActionResult<List<ServiceItemOptions>>> GetServices()
        {
            try
            {
                return await brandingService.GetServicesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving services");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve services");
            }
        }

        // POST /api/branding/services
        [HttpPost("services")]
        public async Task<ActionResult<ServiceItemOptions>> CreateService([FromBody] ServiceItemOptions options)
        {
            try
            {
                var result = await brandingService.CreateServiceAsync(options);
                return CreatedAtAction(nameof(GetServices), result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating service");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create service");
            }
        }

        // PUT /api/branding/services/{id}
        [HttpPut("services/{id}")]
        public async Task<ActionResult> UpdateService(Guid id, [FromBody] ServiceItemOptions options)
        {
            try
            {
                await brandingService.UpdateServiceAsync(id, options);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating service");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update service");
            }
        }

        // DELETE /api/branding/services/{id}
        [HttpDelete("services/{id}")]
        public async Task<ActionResult> DeleteService(Guid id)
        {
            try
            {
                await brandingService.DeleteServiceAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting service");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete service");
            }
        }

        // PUT /api/branding/services/reorder
        [HttpPut("services/reorder")]
        public async Task<ActionResult> ReorderServices([FromBody] List<Guid> orderedIds)
        {
            try
            {
                await brandingService.ReorderServicesAsync(orderedIds);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reordering services");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to reorder services");
            }
        }

        // === About ===

        // GET /api/branding/about
        [HttpGet("about")]
        public async Task<ActionResult<AboutOptions>> GetAbout()
        {
            try
            {
                return await brandingService.GetAboutAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving about");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve about");
            }
        }

        // PUT /api/branding/about
        [HttpPut("about")]
        public async Task<ActionResult> UpdateAbout([FromBody] AboutOptions options)
        {
            try
            {
                await brandingService.SaveAboutAsync(options);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving about");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save about");
            }
        }

        // POST /api/branding/about/features
        [HttpPost("about/features")]
        public async Task<ActionResult<AboutFeatureOptions>> CreateAboutFeature([FromBody] AboutFeatureOptions options)
        {
            try
            {
                var result = await brandingService.CreateAboutFeatureAsync(options);
                return CreatedAtAction(nameof(GetAbout), result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating about feature");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create about feature");
            }
        }

        // PUT /api/branding/about/features/{id}
        [HttpPut("about/features/{id}")]
        public async Task<ActionResult> UpdateAboutFeature(Guid id, [FromBody] AboutFeatureOptions options)
        {
            try
            {
                await brandingService.UpdateAboutFeatureAsync(id, options);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating about feature");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update about feature");
            }
        }

        // DELETE /api/branding/about/features/{id}
        [HttpDelete("about/features/{id}")]
        public async Task<ActionResult> DeleteAboutFeature(Guid id)
        {
            try
            {
                await brandingService.DeleteAboutFeatureAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting about feature");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete about feature");
            }
        }

        // === Stats CRUD ===

        // GET /api/branding/stats
        [HttpGet("stats")]
        public async Task<ActionResult<List<StatItemOptions>>> GetStats()
        {
            try
            {
                return await brandingService.GetStatsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving stats");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve stats");
            }
        }

        // POST /api/branding/stats
        [HttpPost("stats")]
        public async Task<ActionResult<StatItemOptions>> CreateStat([FromBody] StatItemOptions options)
        {
            try
            {
                var result = await brandingService.CreateStatAsync(options);
                return CreatedAtAction(nameof(GetStats), result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating stat");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create stat");
            }
        }

        // PUT /api/branding/stats/{id}
        [HttpPut("stats/{id}")]
        public async Task<ActionResult> UpdateStat(Guid id, [FromBody] StatItemOptions options)
        {
            try
            {
                await brandingService.UpdateStatAsync(id, options);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating stat");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update stat");
            }
        }

        // DELETE /api/branding/stats/{id}
        [HttpDelete("stats/{id}")]
        public async Task<ActionResult> DeleteStat(Guid id)
        {
            try
            {
                await brandingService.DeleteStatAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting stat");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete stat");
            }
        }

        // PUT /api/branding/stats/reorder
        [HttpPut("stats/reorder")]
        public async Task<ActionResult> ReorderStats([FromBody] List<Guid> orderedIds)
        {
            try
            {
                await brandingService.ReorderStatsAsync(orderedIds);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reordering stats");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to reorder stats");
            }
        }

        // === Tips Section ===

        // GET /api/branding/tips-section
        [HttpGet("tips-section")]
        public async Task<ActionResult<TipsSectionOptions>> GetTipsSection()
        {
            try
            {
                return await brandingService.GetTipsSectionAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving tips section");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve tips section");
            }
        }

        // PUT /api/branding/tips-section
        [HttpPut("tips-section")]
        public async Task<ActionResult> UpdateTipsSection([FromBody] TipsSectionOptions options)
        {
            try
            {
                await brandingService.SaveTipsSectionAsync(options);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving tips section");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save tips section");
            }
        }

        // === Tips CRUD ===

        // GET /api/branding/tips
        [HttpGet("tips")]
        public async Task<ActionResult<List<TipItemOptions>>> GetTips()
        {
            try
            {
                return await brandingService.GetTipsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving tips");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve tips");
            }
        }

        // POST /api/branding/tips
        [HttpPost("tips")]
        public async Task<ActionResult<TipItemOptions>> CreateTip([FromBody] TipItemOptions options)
        {
            try
            {
                var result = await brandingService.CreateTipAsync(options);
                return CreatedAtAction(nameof(GetTips), result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating tip");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create tip");
            }
        }

        // PUT /api/branding/tips/{id}
        [HttpPut("tips/{id}")]
        public async Task<ActionResult> UpdateTip(Guid id, [FromBody] TipItemOptions options)
        {
            try
            {
                await brandingService.UpdateTipAsync(id, options);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating tip");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update tip");
            }
        }

        // DELETE /api/branding/tips/{id}
        [HttpDelete("tips/{id}")]
        public async Task<ActionResult> DeleteTip(Guid id)
        {
            try
            {
                await brandingService.DeleteTipAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting tip");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete tip");
            }
        }

        // PUT /api/branding/tips/reorder
        [HttpPut("tips/reorder")]
        public async Task<ActionResult> ReorderTips([FromBody] List<Guid> orderedIds)
        {
            try
            {
                await brandingService.ReorderTipsAsync(orderedIds);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reordering tips");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to reorder tips");
            }
        }

        // === Footer ===

        // GET /api/branding/footer
        [HttpGet("footer")]
        public async Task<ActionResult<FooterOptions>> GetFooter()
        {
            try
            {
                return await brandingService.GetFooterAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving footer");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve footer");
            }
        }

        // PUT /api/branding/footer
        [HttpPut("footer")]
        public async Task<ActionResult> UpdateFooter([FromBody] FooterOptions options)
        {
            try
            {
                await brandingService.SaveFooterAsync(options);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving footer");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save footer");
            }
        }

        // === Contact ===

        // GET /api/branding/contact
        [HttpGet("contact")]
        public async Task<ActionResult<ContactOptions>> GetContact()
        {
            try
            {
                return await brandingService.GetContactAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving contact");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve contact");
            }
        }

        // PUT /api/branding/contact
        [HttpPut("contact")]
        public async Task<ActionResult> UpdateContact([FromBody] ContactOptions options)
        {
            try
            {
                await brandingService.SaveContactAsync(options);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving contact");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save contact");
            }
        }

        // === Section Visibility ===

        // GET /api/branding/section-visibility
        [HttpGet("section-visibility")]
        public async Task<ActionResult<SectionVisibilityOptions>> GetSectionVisibility()
        {
            try
            {
                return await brandingService.GetSectionVisibilityAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving section visibility");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve section visibility");
            }
        }

        // PUT /api/branding/section-visibility
        [HttpPut("section-visibility")]
        public async Task<ActionResult> UpdateSectionVisibility([FromBody] SectionVisibilityOptions options)
        {
            try
            {
                await brandingService.SaveSectionVisibilityAsync(options);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving section visibility");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save section visibility");
            }
        }

        // === Gallery Section ===

        // GET /api/branding/gallery-section
        [HttpGet("gallery-section")]
        public async Task<ActionResult<GallerySectionOptions>> GetGallerySection()
        {
            try
            {
                return await brandingService.GetGallerySectionAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving gallery section");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve gallery section");
            }
        }

        // PUT /api/branding/gallery-section
        [HttpPut("gallery-section")]
        public async Task<ActionResult> UpdateGallerySection([FromBody] GallerySectionOptions options)
        {
            try
            {
                await brandingService.SaveGallerySectionAsync(options);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving gallery section");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save gallery section");
            }
        }

        // === Gallery Photos CRUD ===

        // GET /api/branding/gallery-photos
        [HttpGet("gallery-photos")]
        public async Task<ActionResult<List<GalleryPhotoOptions>>> GetGalleryPhotos()
        {
            try
            {
                return await brandingService.GetGalleryPhotosAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving gallery photos");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve gallery photos");
            }
        }

        // POST /api/branding/gallery-photos
        [HttpPost("gallery-photos")]
        public async Task<ActionResult<GalleryPhotoOptions>> CreateGalleryPhoto([FromBody] GalleryPhotoOptions options)
        {
            try
            {
                var result = await brandingService.CreateGalleryPhotoAsync(options);
                return CreatedAtAction(nameof(GetGalleryPhotos), result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating gallery photo");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create gallery photo");
            }
        }

        // PUT /api/branding/gallery-photos/{id}
        [HttpPut("gallery-photos/{id}")]
        public async Task<ActionResult> UpdateGalleryPhoto(Guid id, [FromBody] GalleryPhotoOptions options)
        {
            try
            {
                await brandingService.UpdateGalleryPhotoAsync(id, options);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating gallery photo");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update gallery photo");
            }
        }

        // DELETE /api/branding/gallery-photos/{id}
        [HttpDelete("gallery-photos/{id}")]
        public async Task<ActionResult> DeleteGalleryPhoto(Guid id)
        {
            try
            {
                await brandingService.DeleteGalleryPhotoAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting gallery photo");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete gallery photo");
            }
        }

        // PUT /api/branding/gallery-photos/reorder
        [HttpPut("gallery-photos/reorder")]
        public async Task<ActionResult> ReorderGalleryPhotos([FromBody] List<Guid> orderedIds)
        {
            try
            {
                await brandingService.ReorderGalleryPhotosAsync(orderedIds);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reordering gallery photos");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to reorder gallery photos");
            }
        }

        // GET /api/branding/gallery-photos/{id}/image (public endpoint)
        [AllowAnonymous]
        [HttpGet("gallery-photos/{id}/image")]
        public async Task<IActionResult> GetGalleryPhotoImage(Guid id)
        {
            try
            {
                var photo = await brandingService.GetGalleryPhotoByIdAsync(id);
                if (photo == null || string.IsNullOrEmpty(photo.ImageBase64))
                {
                    return NotFound();
                }

                var bytes = Convert.FromBase64String(photo.ImageBase64);
                return File(bytes, photo.ImageMimeType ?? "image/jpeg");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving gallery photo image");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve gallery photo image");
            }
        }

        // === Social Links CRUD ===

        // GET /api/branding/social-links
        [HttpGet("social-links")]
        public async Task<ActionResult<List<SocialLinkOptions>>> GetSocialLinks()
        {
            try
            {
                return await brandingService.GetSocialLinksAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving social links");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve social links");
            }
        }

        // POST /api/branding/social-links
        [HttpPost("social-links")]
        public async Task<ActionResult<SocialLinkOptions>> CreateSocialLink([FromBody] SocialLinkOptions options)
        {
            try
            {
                var result = await brandingService.CreateSocialLinkAsync(options);
                return CreatedAtAction(nameof(GetSocialLinks), result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating social link");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create social link");
            }
        }

        // PUT /api/branding/social-links/{id}
        [HttpPut("social-links/{id}")]
        public async Task<ActionResult> UpdateSocialLink(Guid id, [FromBody] SocialLinkOptions options)
        {
            try
            {
                await brandingService.UpdateSocialLinkAsync(id, options);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating social link");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update social link");
            }
        }

        // DELETE /api/branding/social-links/{id}
        [HttpDelete("social-links/{id}")]
        public async Task<ActionResult> DeleteSocialLink(Guid id)
        {
            try
            {
                await brandingService.DeleteSocialLinkAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting social link");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete social link");
            }
        }

        // PUT /api/branding/social-links/reorder
        [HttpPut("social-links/reorder")]
        public async Task<ActionResult> ReorderSocialLinks([FromBody] List<Guid> orderedIds)
        {
            try
            {
                await brandingService.ReorderSocialLinksAsync(orderedIds);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reordering social links");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to reorder social links");
            }
        }

        // === Full Landing Content ===

        // GET /api/branding/landing-content
        [HttpGet("landing-content")]
        public async Task<ActionResult<LandingContentOptions>> GetLandingContent()
        {
            try
            {
                return await brandingService.GetLandingContentAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving landing content");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve landing content");
            }
        }
    }
}
