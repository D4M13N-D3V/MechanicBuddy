using MechanicBuddy.Core.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MechanicBuddy.Core.Application.Services
{
    public interface IBrandingRepository
    {
        // Branding
        Task<TenantBranding> GetBrandingAsync();
        Task SaveBrandingAsync(TenantBranding branding);

        // Hero
        Task<LandingHero> GetHeroAsync();
        Task SaveHeroAsync(LandingHero hero);

        // Services
        Task<IList<LandingService>> GetServicesAsync();
        Task<LandingService> GetServiceByIdAsync(Guid id);
        Task SaveServiceAsync(LandingService service);
        Task DeleteServiceAsync(Guid id);

        // About
        Task<LandingAbout> GetAboutAsync();
        Task SaveAboutAsync(LandingAbout about);
        Task<LandingAboutFeature> GetAboutFeatureByIdAsync(Guid id);
        Task SaveAboutFeatureAsync(LandingAboutFeature feature);
        Task DeleteAboutFeatureAsync(Guid id);

        // Stats
        Task<IList<LandingStat>> GetStatsAsync();
        Task<LandingStat> GetStatByIdAsync(Guid id);
        Task SaveStatAsync(LandingStat stat);
        Task DeleteStatAsync(Guid id);

        // Tips Section
        Task<LandingTipsSection> GetTipsSectionAsync();
        Task SaveTipsSectionAsync(LandingTipsSection tipsSection);

        // Tips
        Task<IList<LandingTip>> GetTipsAsync();
        Task<LandingTip> GetTipByIdAsync(Guid id);
        Task SaveTipAsync(LandingTip tip);
        Task DeleteTipAsync(Guid id);

        // Footer
        Task<LandingFooter> GetFooterAsync();
        Task SaveFooterAsync(LandingFooter footer);

        // Contact
        Task<LandingContact> GetContactAsync();
        Task SaveContactAsync(LandingContact contact);

        // Section Visibility
        Task<LandingSectionVisibility> GetSectionVisibilityAsync();
        Task SaveSectionVisibilityAsync(LandingSectionVisibility visibility);

        // Gallery Section
        Task<LandingGallerySection> GetGallerySectionAsync();
        Task SaveGallerySectionAsync(LandingGallerySection gallerySection);

        // Gallery Photos
        Task<IList<LandingGalleryPhoto>> GetGalleryPhotosAsync();
        Task<LandingGalleryPhoto> GetGalleryPhotoByIdAsync(Guid id);
        Task SaveGalleryPhotoAsync(LandingGalleryPhoto photo);
        Task DeleteGalleryPhotoAsync(Guid id);

        // Social Links
        Task<IList<LandingSocialLink>> GetSocialLinksAsync();
        Task<LandingSocialLink> GetSocialLinkByIdAsync(Guid id);
        Task SaveSocialLinkAsync(LandingSocialLink socialLink);
        Task DeleteSocialLinkAsync(Guid id);
    }
}
