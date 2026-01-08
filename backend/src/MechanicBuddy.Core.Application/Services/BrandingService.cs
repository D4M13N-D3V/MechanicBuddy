using MechanicBuddy.Core.Application.Configuration;
using MechanicBuddy.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MechanicBuddy.Core.Application.Services
{
    public class BrandingService : IBrandingService
    {
        private readonly IBrandingRepository repository;
        private readonly ITenantConfigRepository tenantConfigRepository;

        public BrandingService(IBrandingRepository repository, ITenantConfigRepository tenantConfigRepository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.tenantConfigRepository = tenantConfigRepository ?? throw new ArgumentNullException(nameof(tenantConfigRepository));
        }

        // Branding
        public async Task<BrandingOptions> GetBrandingAsync()
        {
            var branding = await repository.GetBrandingAsync();

            return new BrandingOptions(
                branding.Logo != null ? Convert.ToBase64String(branding.Logo) : null,
                branding.LogoMimeType,
                new PortalColorsOptions(
                    branding.PortalSidebarBg,
                    branding.PortalSidebarText,
                    branding.PortalSidebarActiveBg,
                    branding.PortalSidebarActiveText,
                    branding.PortalAccentColor,
                    branding.PortalContentBg
                ),
                new LandingColorsOptions(
                    branding.LandingPrimaryColor,
                    branding.LandingSecondaryColor,
                    branding.LandingAccentColor,
                    branding.LandingHeaderBg,
                    branding.LandingFooterBg
                )
            );
        }

        public async Task SaveBrandingAsync(BrandingOptions options)
        {
            var branding = await repository.GetBrandingAsync();

            if (!string.IsNullOrEmpty(options.LogoBase64))
            {
                branding.UpdateLogo(Convert.FromBase64String(options.LogoBase64), options.LogoMimeType);
            }

            branding.UpdatePortalColors(
                options.PortalColors?.SidebarBg,
                options.PortalColors?.SidebarText,
                options.PortalColors?.SidebarActiveBg,
                options.PortalColors?.SidebarActiveText,
                options.PortalColors?.AccentColor,
                options.PortalColors?.ContentBg
            );

            branding.UpdateLandingColors(
                options.LandingColors?.PrimaryColor,
                options.LandingColors?.SecondaryColor,
                options.LandingColors?.AccentColor,
                options.LandingColors?.HeaderBg,
                options.LandingColors?.FooterBg
            );

            await repository.SaveBrandingAsync(branding);
        }

        public async Task<byte[]> GetLogoAsync()
        {
            var branding = await repository.GetBrandingAsync();
            return branding.Logo;
        }

        // Hero
        public async Task<HeroOptions> GetHeroAsync()
        {
            var hero = await repository.GetHeroAsync();

            return new HeroOptions(
                hero.CompanyName,
                hero.Tagline,
                hero.Subtitle,
                hero.SpecialtyText,
                hero.CtaPrimaryText,
                hero.CtaPrimaryLink,
                hero.CtaSecondaryText,
                hero.CtaSecondaryLink,
                hero.BackgroundImage != null ? Convert.ToBase64String(hero.BackgroundImage) : null,
                hero.BackgroundImageMimeType
            );
        }

        public async Task SaveHeroAsync(HeroOptions options)
        {
            var hero = await repository.GetHeroAsync();

            hero.Update(
                options.CompanyName,
                options.Tagline,
                options.Subtitle,
                options.SpecialtyText,
                options.CtaPrimaryText,
                options.CtaPrimaryLink,
                options.CtaSecondaryText,
                options.CtaSecondaryLink
            );

            if (!string.IsNullOrEmpty(options.BackgroundImageBase64))
            {
                hero.UpdateBackgroundImage(
                    Convert.FromBase64String(options.BackgroundImageBase64),
                    options.BackgroundImageMimeType
                );
            }

            await repository.SaveHeroAsync(hero);
        }

        public async Task<byte[]> GetHeroBackgroundImageAsync()
        {
            var hero = await repository.GetHeroAsync();
            return hero.BackgroundImage;
        }

        // Services
        public async Task<List<ServiceItemOptions>> GetServicesAsync()
        {
            var services = await repository.GetServicesAsync();
            return services.Select(s => new ServiceItemOptions(
                s.Id,
                s.IconName,
                s.Title,
                s.Description,
                s.UsePrimaryColor,
                s.SortOrder,
                s.IsActive
            )).ToList();
        }

        public async Task<ServiceItemOptions> GetServiceByIdAsync(Guid id)
        {
            var service = await repository.GetServiceByIdAsync(id);
            if (service == null) return null;

            return new ServiceItemOptions(
                service.Id,
                service.IconName,
                service.Title,
                service.Description,
                service.UsePrimaryColor,
                service.SortOrder,
                service.IsActive
            );
        }

        public async Task<ServiceItemOptions> CreateServiceAsync(ServiceItemOptions options)
        {
            var services = await repository.GetServicesAsync();
            var maxSortOrder = services.Any() ? services.Max(s => s.SortOrder) : -1;

            var service = new LandingService(
                options.IconName,
                options.Title,
                options.Description,
                options.UsePrimaryColor,
                maxSortOrder + 1
            );

            await repository.SaveServiceAsync(service);

            return new ServiceItemOptions(
                service.Id,
                service.IconName,
                service.Title,
                service.Description,
                service.UsePrimaryColor,
                service.SortOrder,
                service.IsActive
            );
        }

        public async Task UpdateServiceAsync(Guid id, ServiceItemOptions options)
        {
            var service = await repository.GetServiceByIdAsync(id);
            if (service == null)
                throw new InvalidOperationException($"Service with id {id} not found");

            service.Update(
                options.IconName,
                options.Title,
                options.Description,
                options.UsePrimaryColor,
                options.SortOrder,
                options.IsActive
            );

            await repository.SaveServiceAsync(service);
        }

        public async Task DeleteServiceAsync(Guid id)
        {
            await repository.DeleteServiceAsync(id);
        }

        public async Task ReorderServicesAsync(List<Guid> orderedIds)
        {
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var service = await repository.GetServiceByIdAsync(orderedIds[i]);
                if (service != null)
                {
                    service.UpdateSortOrder(i);
                    await repository.SaveServiceAsync(service);
                }
            }
        }

        // About
        public async Task<AboutOptions> GetAboutAsync()
        {
            var about = await repository.GetAboutAsync();

            var features = about.Features?
                .OrderBy(f => f.SortOrder)
                .Select(f => new AboutFeatureOptions(f.Id, f.Text, f.SortOrder))
                .ToList() ?? new List<AboutFeatureOptions>();

            return new AboutOptions(
                about.SectionLabel,
                about.Headline,
                about.Description,
                about.SecondaryDescription,
                features
            );
        }

        public async Task SaveAboutAsync(AboutOptions options)
        {
            var about = await repository.GetAboutAsync();

            about.Update(
                options.Headline,
                options.Description,
                options.SecondaryDescription,
                options.SectionLabel
            );

            await repository.SaveAboutAsync(about);
        }

        public async Task<AboutFeatureOptions> CreateAboutFeatureAsync(AboutFeatureOptions options)
        {
            var about = await repository.GetAboutAsync();
            var feature = about.AddFeature(options.Text, options.SortOrder);
            await repository.SaveAboutAsync(about);

            return new AboutFeatureOptions(feature.Id, feature.Text, feature.SortOrder);
        }

        public async Task UpdateAboutFeatureAsync(Guid id, AboutFeatureOptions options)
        {
            var feature = await repository.GetAboutFeatureByIdAsync(id);
            if (feature == null)
                throw new InvalidOperationException($"About feature with id {id} not found");

            feature.Update(options.Text, options.SortOrder);
            await repository.SaveAboutFeatureAsync(feature);
        }

        public async Task DeleteAboutFeatureAsync(Guid id)
        {
            await repository.DeleteAboutFeatureAsync(id);
        }

        // Stats
        public async Task<List<StatItemOptions>> GetStatsAsync()
        {
            var stats = await repository.GetStatsAsync();
            return stats.Select(s => new StatItemOptions(
                s.Id,
                s.Value,
                s.Label,
                s.SortOrder
            )).ToList();
        }

        public async Task<StatItemOptions> GetStatByIdAsync(Guid id)
        {
            var stat = await repository.GetStatByIdAsync(id);
            if (stat == null) return null;

            return new StatItemOptions(stat.Id, stat.Value, stat.Label, stat.SortOrder);
        }

        public async Task<StatItemOptions> CreateStatAsync(StatItemOptions options)
        {
            var stats = await repository.GetStatsAsync();
            var maxSortOrder = stats.Any() ? stats.Max(s => s.SortOrder) : -1;

            var stat = new LandingStat(options.Value, options.Label, maxSortOrder + 1);
            await repository.SaveStatAsync(stat);

            return new StatItemOptions(stat.Id, stat.Value, stat.Label, stat.SortOrder);
        }

        public async Task UpdateStatAsync(Guid id, StatItemOptions options)
        {
            var stat = await repository.GetStatByIdAsync(id);
            if (stat == null)
                throw new InvalidOperationException($"Stat with id {id} not found");

            stat.Update(options.Value, options.Label, options.SortOrder);
            await repository.SaveStatAsync(stat);
        }

        public async Task DeleteStatAsync(Guid id)
        {
            await repository.DeleteStatAsync(id);
        }

        public async Task ReorderStatsAsync(List<Guid> orderedIds)
        {
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var stat = await repository.GetStatByIdAsync(orderedIds[i]);
                if (stat != null)
                {
                    stat.UpdateSortOrder(i);
                    await repository.SaveStatAsync(stat);
                }
            }
        }

        // Tips Section
        public async Task<TipsSectionOptions> GetTipsSectionAsync()
        {
            var section = await repository.GetTipsSectionAsync();
            return new TipsSectionOptions(
                section.IsVisible,
                section.SectionLabel,
                section.Headline,
                section.Description
            );
        }

        public async Task SaveTipsSectionAsync(TipsSectionOptions options)
        {
            var section = await repository.GetTipsSectionAsync();
            section.Update(options.IsVisible, options.SectionLabel, options.Headline, options.Description);
            await repository.SaveTipsSectionAsync(section);
        }

        // Tips
        public async Task<List<TipItemOptions>> GetTipsAsync()
        {
            var tips = await repository.GetTipsAsync();
            return tips.Select(t => new TipItemOptions(
                t.Id,
                t.Title,
                t.Description,
                t.SortOrder,
                t.IsActive
            )).ToList();
        }

        public async Task<TipItemOptions> GetTipByIdAsync(Guid id)
        {
            var tip = await repository.GetTipByIdAsync(id);
            if (tip == null) return null;

            return new TipItemOptions(tip.Id, tip.Title, tip.Description, tip.SortOrder, tip.IsActive);
        }

        public async Task<TipItemOptions> CreateTipAsync(TipItemOptions options)
        {
            var tips = await repository.GetTipsAsync();
            var maxSortOrder = tips.Any() ? tips.Max(t => t.SortOrder) : -1;

            var tip = new LandingTip(options.Title, options.Description, maxSortOrder + 1);
            await repository.SaveTipAsync(tip);

            return new TipItemOptions(tip.Id, tip.Title, tip.Description, tip.SortOrder, tip.IsActive);
        }

        public async Task UpdateTipAsync(Guid id, TipItemOptions options)
        {
            var tip = await repository.GetTipByIdAsync(id);
            if (tip == null)
                throw new InvalidOperationException($"Tip with id {id} not found");

            tip.Update(options.Title, options.Description, options.SortOrder, options.IsActive);
            await repository.SaveTipAsync(tip);
        }

        public async Task DeleteTipAsync(Guid id)
        {
            await repository.DeleteTipAsync(id);
        }

        public async Task ReorderTipsAsync(List<Guid> orderedIds)
        {
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var tip = await repository.GetTipByIdAsync(orderedIds[i]);
                if (tip != null)
                {
                    tip.UpdateSortOrder(i);
                    await repository.SaveTipAsync(tip);
                }
            }
        }

        // Footer
        public async Task<FooterOptions> GetFooterAsync()
        {
            var footer = await repository.GetFooterAsync();
            return new FooterOptions(
                footer.CompanyDescription,
                footer.ShowQuickLinks,
                footer.ShowContactInfo,
                footer.CopyrightText
            );
        }

        public async Task SaveFooterAsync(FooterOptions options)
        {
            var footer = await repository.GetFooterAsync();
            footer.Update(
                options.CompanyDescription,
                options.ShowQuickLinks,
                options.ShowContactInfo,
                options.CopyrightText
            );
            await repository.SaveFooterAsync(footer);
        }

        // Contact
        public async Task<ContactOptions> GetContactAsync()
        {
            var contact = await repository.GetContactAsync();

            List<BusinessHoursEntry> businessHours = null;
            if (!string.IsNullOrEmpty(contact.BusinessHours))
            {
                try
                {
                    businessHours = JsonSerializer.Deserialize<List<BusinessHoursEntry>>(contact.BusinessHours);
                }
                catch
                {
                    businessHours = new List<BusinessHoursEntry>();
                }
            }

            return new ContactOptions(
                contact.SectionLabel,
                contact.Headline,
                contact.Description,
                contact.ShowTowing,
                contact.TowingText,
                businessHours ?? new List<BusinessHoursEntry>()
            );
        }

        public async Task SaveContactAsync(ContactOptions options)
        {
            var contact = await repository.GetContactAsync();

            string businessHoursJson = null;
            if (options.BusinessHours != null && options.BusinessHours.Any())
            {
                businessHoursJson = JsonSerializer.Serialize(options.BusinessHours);
            }

            contact.Update(
                options.SectionLabel,
                options.Headline,
                options.Description,
                options.ShowTowing,
                options.TowingText,
                businessHoursJson
            );

            await repository.SaveContactAsync(contact);
        }

        // Section Visibility
        public async Task<SectionVisibilityOptions> GetSectionVisibilityAsync()
        {
            var visibility = await repository.GetSectionVisibilityAsync();
            return new SectionVisibilityOptions(
                visibility.HeroVisible,
                visibility.ServicesVisible,
                visibility.AboutVisible,
                visibility.StatsVisible,
                visibility.TipsVisible,
                visibility.GalleryVisible,
                visibility.ContactVisible
            );
        }

        public async Task SaveSectionVisibilityAsync(SectionVisibilityOptions options)
        {
            var visibility = await repository.GetSectionVisibilityAsync();
            visibility.Update(
                options.HeroVisible,
                options.ServicesVisible,
                options.AboutVisible,
                options.StatsVisible,
                options.TipsVisible,
                options.GalleryVisible,
                options.ContactVisible
            );
            await repository.SaveSectionVisibilityAsync(visibility);
        }

        // Gallery Section
        public async Task<GallerySectionOptions> GetGallerySectionAsync()
        {
            var section = await repository.GetGallerySectionAsync();
            return new GallerySectionOptions(
                section.SectionLabel,
                section.Headline,
                section.Description
            );
        }

        public async Task SaveGallerySectionAsync(GallerySectionOptions options)
        {
            var section = await repository.GetGallerySectionAsync();
            section.Update(options.SectionLabel, options.Headline, options.Description);
            await repository.SaveGallerySectionAsync(section);
        }

        // Gallery Photos
        public async Task<List<GalleryPhotoOptions>> GetGalleryPhotosAsync()
        {
            var photos = await repository.GetGalleryPhotosAsync();
            return photos.Select(p => new GalleryPhotoOptions(
                p.Id,
                p.Image != null ? Convert.ToBase64String(p.Image) : null,
                p.ImageMimeType,
                p.Caption,
                p.SortOrder,
                p.IsActive
            )).ToList();
        }

        // Gallery Photo Metadata (lightweight - no image data)
        public async Task<List<GalleryPhotoMetadata>> GetGalleryPhotoMetadataListAsync()
        {
            return (await repository.GetGalleryPhotoMetadataAsync()).ToList();
        }

        public async Task<GalleryPhotoOptions> GetGalleryPhotoByIdAsync(Guid id)
        {
            var photo = await repository.GetGalleryPhotoByIdAsync(id);
            if (photo == null) return null;

            return new GalleryPhotoOptions(
                photo.Id,
                photo.Image != null ? Convert.ToBase64String(photo.Image) : null,
                photo.ImageMimeType,
                photo.Caption,
                photo.SortOrder,
                photo.IsActive
            );
        }

        public async Task<GalleryPhotoOptions> CreateGalleryPhotoAsync(GalleryPhotoOptions options)
        {
            var photos = await repository.GetGalleryPhotosAsync();
            var maxSortOrder = photos.Any() ? photos.Max(p => p.SortOrder) : -1;

            var imageBytes = !string.IsNullOrEmpty(options.ImageBase64)
                ? Convert.FromBase64String(options.ImageBase64)
                : throw new ArgumentException("Image is required");

            var photo = new LandingGalleryPhoto(
                imageBytes,
                options.ImageMimeType,
                options.Caption,
                maxSortOrder + 1
            );

            await repository.SaveGalleryPhotoAsync(photo);

            return new GalleryPhotoOptions(
                photo.Id,
                Convert.ToBase64String(photo.Image),
                photo.ImageMimeType,
                photo.Caption,
                photo.SortOrder,
                photo.IsActive
            );
        }

        public async Task UpdateGalleryPhotoAsync(Guid id, GalleryPhotoOptions options)
        {
            var photo = await repository.GetGalleryPhotoByIdAsync(id);
            if (photo == null)
                throw new InvalidOperationException($"Gallery photo with id {id} not found");

            byte[] imageBytes = null;
            if (!string.IsNullOrEmpty(options.ImageBase64))
            {
                imageBytes = Convert.FromBase64String(options.ImageBase64);
            }

            photo.Update(
                imageBytes,
                options.ImageMimeType,
                options.Caption,
                options.SortOrder,
                options.IsActive
            );

            await repository.SaveGalleryPhotoAsync(photo);
        }

        public async Task DeleteGalleryPhotoAsync(Guid id)
        {
            await repository.DeleteGalleryPhotoAsync(id);
        }

        public async Task ReorderGalleryPhotosAsync(List<Guid> orderedIds)
        {
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var photo = await repository.GetGalleryPhotoByIdAsync(orderedIds[i]);
                if (photo != null)
                {
                    photo.UpdateSortOrder(i);
                    await repository.SaveGalleryPhotoAsync(photo);
                }
            }
        }

        public async Task<byte[]> GetGalleryPhotoImageAsync(Guid id)
        {
            var photo = await repository.GetGalleryPhotoByIdAsync(id);
            return photo?.Image;
        }

        // Social Links
        public async Task<List<SocialLinkOptions>> GetSocialLinksAsync()
        {
            var links = await repository.GetSocialLinksAsync();
            return links.Select(l => new SocialLinkOptions(
                l.Id,
                l.Platform,
                l.Url,
                l.DisplayName,
                l.IconName,
                l.SortOrder,
                l.IsActive,
                l.ShowInHeader,
                l.ShowInFooter
            )).ToList();
        }

        public async Task<SocialLinkOptions> GetSocialLinkByIdAsync(Guid id)
        {
            var link = await repository.GetSocialLinkByIdAsync(id);
            if (link == null) return null;

            return new SocialLinkOptions(
                link.Id,
                link.Platform,
                link.Url,
                link.DisplayName,
                link.IconName,
                link.SortOrder,
                link.IsActive,
                link.ShowInHeader,
                link.ShowInFooter
            );
        }

        public async Task<SocialLinkOptions> CreateSocialLinkAsync(SocialLinkOptions options)
        {
            var links = await repository.GetSocialLinksAsync();
            var maxSortOrder = links.Any() ? links.Max(l => l.SortOrder) : -1;

            var link = new LandingSocialLink(
                options.Platform,
                options.Url,
                options.DisplayName,
                options.IconName,
                maxSortOrder + 1,
                options.ShowInHeader,
                options.ShowInFooter
            );

            await repository.SaveSocialLinkAsync(link);

            return new SocialLinkOptions(
                link.Id,
                link.Platform,
                link.Url,
                link.DisplayName,
                link.IconName,
                link.SortOrder,
                link.IsActive,
                link.ShowInHeader,
                link.ShowInFooter
            );
        }

        public async Task UpdateSocialLinkAsync(Guid id, SocialLinkOptions options)
        {
            var link = await repository.GetSocialLinkByIdAsync(id);
            if (link == null)
                throw new InvalidOperationException($"Social link with id {id} not found");

            link.Update(
                options.Platform,
                options.Url,
                options.DisplayName,
                options.IconName,
                options.SortOrder,
                options.IsActive,
                options.ShowInHeader,
                options.ShowInFooter
            );

            await repository.SaveSocialLinkAsync(link);
        }

        public async Task DeleteSocialLinkAsync(Guid id)
        {
            await repository.DeleteSocialLinkAsync(id);
        }

        public async Task ReorderSocialLinksAsync(List<Guid> orderedIds)
        {
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var link = await repository.GetSocialLinkByIdAsync(orderedIds[i]);
                if (link != null)
                {
                    link.UpdateSortOrder(i);
                    await repository.SaveSocialLinkAsync(link);
                }
            }
        }

        // Full Content
        // Note: Gallery photos are NOT included to avoid OOM - fetch them separately via GetGalleryPhotoMetadataAsync
        public async Task<LandingContentOptions> GetLandingContentAsync()
        {
            var hero = await GetHeroAsync();
            var services = await GetServicesAsync();
            var about = await GetAboutAsync();
            var stats = await GetStatsAsync();
            var tipsSection = await GetTipsSectionAsync();
            var tips = await GetTipsAsync();
            var footer = await GetFooterAsync();
            var contact = await GetContactAsync();
            var sectionVisibility = await GetSectionVisibilityAsync();
            var gallerySection = await GetGallerySectionAsync();
            // Don't load gallery photos here - they can be huge and cause OOM
            // Frontend should fetch gallery photo list separately
            var galleryPhotos = new List<GalleryPhotoOptions>();
            var socialLinks = await GetSocialLinksAsync();

            return new LandingContentOptions(
                hero,
                services,
                about,
                stats,
                tipsSection,
                tips,
                footer,
                contact,
                sectionVisibility,
                gallerySection,
                galleryPhotos,
                socialLinks
            );
        }

        // Get gallery photos with URLs instead of base64 (lightweight for public)
        // Uses metadata query that does NOT load the image binary data
        public async Task<List<PublicGalleryPhotoOptions>> GetPublicGalleryPhotosAsync(string baseUrl)
        {
            var photos = await repository.GetGalleryPhotoMetadataAsync();
            return photos
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .Select(p => new PublicGalleryPhotoOptions(
                    p.Id,
                    $"{baseUrl}/api/branding/gallery-photos/{p.Id}/image",
                    p.Caption ?? "",
                    p.SortOrder
                ))
                .ToList();
        }

        // Get public landing content (lightweight - no base64 images)
        public async Task<PublicLandingContentOptions> GetPublicLandingContentAsync(string baseUrl)
        {
            var hero = await GetHeroAsync();
            var services = await GetServicesAsync();
            var about = await GetAboutAsync();
            var stats = await GetStatsAsync();
            var tipsSection = await GetTipsSectionAsync();
            var tips = await GetTipsAsync();
            var footer = await GetFooterAsync();
            var contact = await GetContactAsync();
            var sectionVisibility = await GetSectionVisibilityAsync();
            var gallerySection = await GetGallerySectionAsync();
            var galleryPhotos = await GetPublicGalleryPhotosAsync(baseUrl);
            var socialLinks = await GetSocialLinksAsync();

            return new PublicLandingContentOptions(
                hero,
                services,
                about,
                stats,
                tipsSection,
                tips,
                footer,
                contact,
                sectionVisibility,
                gallerySection,
                galleryPhotos,
                socialLinks
            );
        }

        public async Task<PublicLandingData> GetPublicLandingDataAsync(string baseUrl)
        {
            var branding = await GetBrandingAsync();
            var content = await GetPublicLandingContentAsync(baseUrl);
            var requisites = await tenantConfigRepository.GetRequisitesAsync();

            var companyInfo = new RequisitesOptions(
                requisites.Name,
                requisites.Phone,
                requisites.Address,
                requisites.Email,
                requisites.BankAccount,
                requisites.RegNr,
                requisites.KMKR
            );

            return new PublicLandingData(branding, content, companyInfo);
        }
    }
}
