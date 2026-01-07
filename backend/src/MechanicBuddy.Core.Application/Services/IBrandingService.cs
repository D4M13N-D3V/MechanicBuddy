using MechanicBuddy.Core.Application.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MechanicBuddy.Core.Application.Services
{
    public interface IBrandingService
    {
        // Branding
        Task<BrandingOptions> GetBrandingAsync();
        Task SaveBrandingAsync(BrandingOptions options);
        Task<byte[]> GetLogoAsync();

        // Hero
        Task<HeroOptions> GetHeroAsync();
        Task SaveHeroAsync(HeroOptions options);
        Task<byte[]> GetHeroBackgroundImageAsync();

        // Services
        Task<List<ServiceItemOptions>> GetServicesAsync();
        Task<ServiceItemOptions> GetServiceByIdAsync(Guid id);
        Task<ServiceItemOptions> CreateServiceAsync(ServiceItemOptions options);
        Task UpdateServiceAsync(Guid id, ServiceItemOptions options);
        Task DeleteServiceAsync(Guid id);
        Task ReorderServicesAsync(List<Guid> orderedIds);

        // About
        Task<AboutOptions> GetAboutAsync();
        Task SaveAboutAsync(AboutOptions options);
        Task<AboutFeatureOptions> CreateAboutFeatureAsync(AboutFeatureOptions options);
        Task UpdateAboutFeatureAsync(Guid id, AboutFeatureOptions options);
        Task DeleteAboutFeatureAsync(Guid id);

        // Stats
        Task<List<StatItemOptions>> GetStatsAsync();
        Task<StatItemOptions> GetStatByIdAsync(Guid id);
        Task<StatItemOptions> CreateStatAsync(StatItemOptions options);
        Task UpdateStatAsync(Guid id, StatItemOptions options);
        Task DeleteStatAsync(Guid id);
        Task ReorderStatsAsync(List<Guid> orderedIds);

        // Tips Section
        Task<TipsSectionOptions> GetTipsSectionAsync();
        Task SaveTipsSectionAsync(TipsSectionOptions options);

        // Tips
        Task<List<TipItemOptions>> GetTipsAsync();
        Task<TipItemOptions> GetTipByIdAsync(Guid id);
        Task<TipItemOptions> CreateTipAsync(TipItemOptions options);
        Task UpdateTipAsync(Guid id, TipItemOptions options);
        Task DeleteTipAsync(Guid id);
        Task ReorderTipsAsync(List<Guid> orderedIds);

        // Footer
        Task<FooterOptions> GetFooterAsync();
        Task SaveFooterAsync(FooterOptions options);

        // Contact
        Task<ContactOptions> GetContactAsync();
        Task SaveContactAsync(ContactOptions options);

        // Section Visibility
        Task<SectionVisibilityOptions> GetSectionVisibilityAsync();
        Task SaveSectionVisibilityAsync(SectionVisibilityOptions options);

        // Gallery Section
        Task<GallerySectionOptions> GetGallerySectionAsync();
        Task SaveGallerySectionAsync(GallerySectionOptions options);

        // Gallery Photos
        Task<List<GalleryPhotoOptions>> GetGalleryPhotosAsync();
        Task<GalleryPhotoOptions> GetGalleryPhotoByIdAsync(Guid id);
        Task<GalleryPhotoOptions> CreateGalleryPhotoAsync(GalleryPhotoOptions options);
        Task UpdateGalleryPhotoAsync(Guid id, GalleryPhotoOptions options);
        Task DeleteGalleryPhotoAsync(Guid id);
        Task ReorderGalleryPhotosAsync(List<Guid> orderedIds);
        Task<byte[]> GetGalleryPhotoImageAsync(Guid id);

        // Social Links
        Task<List<SocialLinkOptions>> GetSocialLinksAsync();
        Task<SocialLinkOptions> GetSocialLinkByIdAsync(Guid id);
        Task<SocialLinkOptions> CreateSocialLinkAsync(SocialLinkOptions options);
        Task UpdateSocialLinkAsync(Guid id, SocialLinkOptions options);
        Task DeleteSocialLinkAsync(Guid id);
        Task ReorderSocialLinksAsync(List<Guid> orderedIds);

        // Full Content
        Task<LandingContentOptions> GetLandingContentAsync();
        Task<PublicLandingContentOptions> GetPublicLandingContentAsync(string baseUrl);
        Task<PublicLandingData> GetPublicLandingDataAsync(string baseUrl);
    }
}
