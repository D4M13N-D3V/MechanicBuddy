using System;
using System.Collections.Generic;

namespace MechanicBuddy.Core.Application.Configuration
{
    // Portal Colors
    public record PortalColorsOptions(
        string SidebarBg,
        string SidebarText,
        string SidebarActiveBg,
        string SidebarActiveText,
        string AccentColor,
        string ContentBg
    );

    // Landing Colors
    public record LandingColorsOptions(
        string PrimaryColor,
        string SecondaryColor,
        string AccentColor,
        string HeaderBg,
        string FooterBg
    );

    // Full Branding Options
    public record BrandingOptions(
        string LogoBase64,
        string LogoMimeType,
        PortalColorsOptions PortalColors,
        LandingColorsOptions LandingColors
    );

    // Hero Section
    public record HeroOptions(
        string CompanyName,
        string Tagline,
        string Subtitle,
        string SpecialtyText,
        string CtaPrimaryText,
        string CtaPrimaryLink,
        string CtaSecondaryText,
        string CtaSecondaryLink,
        string BackgroundImageBase64,
        string BackgroundImageMimeType
    );

    // Service Item
    public record ServiceItemOptions(
        Guid? Id,
        string IconName,
        string Title,
        string Description,
        bool UsePrimaryColor,
        int SortOrder,
        bool IsActive
    );

    // About Section
    public record AboutOptions(
        string SectionLabel,
        string Headline,
        string Description,
        string SecondaryDescription,
        List<AboutFeatureOptions> Features
    );

    // About Feature
    public record AboutFeatureOptions(
        Guid? Id,
        string Text,
        int SortOrder
    );

    // Stat Item
    public record StatItemOptions(
        Guid? Id,
        string Value,
        string Label,
        int SortOrder
    );

    // Tips Section Settings
    public record TipsSectionOptions(
        bool IsVisible,
        string SectionLabel,
        string Headline,
        string Description
    );

    // Tip Item
    public record TipItemOptions(
        Guid? Id,
        string Title,
        string Description,
        int SortOrder,
        bool IsActive
    );

    // Footer Settings
    public record FooterOptions(
        string CompanyDescription,
        bool ShowQuickLinks,
        bool ShowContactInfo,
        string CopyrightText
    );

    // Business Hours Entry
    public record BusinessHoursEntry(
        string Day,
        string Open,
        string Close
    );

    // Contact Section
    public record ContactOptions(
        string SectionLabel,
        string Headline,
        string Description,
        bool ShowTowing,
        string TowingText,
        List<BusinessHoursEntry> BusinessHours
    );

    // Section Visibility
    public record SectionVisibilityOptions(
        bool HeroVisible,
        bool ServicesVisible,
        bool AboutVisible,
        bool StatsVisible,
        bool TipsVisible,
        bool GalleryVisible,
        bool ContactVisible
    );

    // Gallery Section Settings
    public record GallerySectionOptions(
        string SectionLabel,
        string Headline,
        string Description
    );

    // Gallery Photo Item (with full image data for admin)
    public record GalleryPhotoOptions(
        Guid? Id,
        string ImageBase64,
        string ImageMimeType,
        string Caption,
        int SortOrder,
        bool IsActive
    );

    // Gallery Photo Item (lightweight for public landing page - uses URL instead of base64)
    public record PublicGalleryPhotoOptions(
        Guid Id,
        string ImageUrl,
        string Caption,
        int SortOrder
    );

    // Gallery Photo Metadata (for listing without loading binary data)
    public record GalleryPhotoMetadata(
        Guid Id,
        string Caption,
        int SortOrder,
        bool IsActive
    );

    // Social Link Item
    public record SocialLinkOptions(
        Guid? Id,
        string Platform,
        string Url,
        string DisplayName,
        string IconName,
        int SortOrder,
        bool IsActive,
        bool ShowInHeader,
        bool ShowInFooter
    );

    // Full Landing Content Options
    public record LandingContentOptions(
        HeroOptions Hero,
        List<ServiceItemOptions> Services,
        AboutOptions About,
        List<StatItemOptions> Stats,
        TipsSectionOptions TipsSection,
        List<TipItemOptions> Tips,
        FooterOptions Footer,
        ContactOptions Contact,
        SectionVisibilityOptions SectionVisibility,
        GallerySectionOptions GallerySection,
        List<GalleryPhotoOptions> GalleryPhotos,
        List<SocialLinkOptions> SocialLinks
    );

    // Public Landing Content Options (lightweight - no base64 images)
    public record PublicLandingContentOptions(
        HeroOptions Hero,
        List<ServiceItemOptions> Services,
        AboutOptions About,
        List<StatItemOptions> Stats,
        TipsSectionOptions TipsSection,
        List<TipItemOptions> Tips,
        FooterOptions Footer,
        ContactOptions Contact,
        SectionVisibilityOptions SectionVisibility,
        GallerySectionOptions GallerySection,
        List<PublicGalleryPhotoOptions> GalleryPhotos,
        List<SocialLinkOptions> SocialLinks
    );

    // Public Landing Page Data (for unauthenticated access)
    public record PublicLandingData(
        BrandingOptions Branding,
        PublicLandingContentOptions Content,
        RequisitesOptions CompanyInfo
    );
}
