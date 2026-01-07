using System;

namespace MechanicBuddy.Core.Domain
{
    public class TenantBranding : GuidIdentityEntity
    {
        // Logo
        public virtual byte[] Logo { get; protected set; }
        public virtual string LogoMimeType { get; protected set; }

        // Portal Colors
        public virtual string PortalSidebarBg { get; protected set; }
        public virtual string PortalSidebarText { get; protected set; }
        public virtual string PortalSidebarActiveBg { get; protected set; }
        public virtual string PortalSidebarActiveText { get; protected set; }
        public virtual string PortalAccentColor { get; protected set; }
        public virtual string PortalContentBg { get; protected set; }

        // Landing Page Colors
        public virtual string LandingPrimaryColor { get; protected set; }
        public virtual string LandingSecondaryColor { get; protected set; }
        public virtual string LandingAccentColor { get; protected set; }
        public virtual string LandingHeaderBg { get; protected set; }
        public virtual string LandingFooterBg { get; protected set; }

        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected TenantBranding() { }

        public TenantBranding(Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            // Default colors
            PortalSidebarBg = "#111827";
            PortalSidebarText = "#9ca3af";
            PortalSidebarActiveBg = "#1f2937";
            PortalSidebarActiveText = "#ffffff";
            PortalAccentColor = "#4f46e5";
            PortalContentBg = "#f9fafb";
            LandingPrimaryColor = "#7c3aed";
            LandingSecondaryColor = "#22c55e";
            LandingAccentColor = "#5b21b6";
            LandingHeaderBg = "#0f172a";
            LandingFooterBg = "#0f172a";
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void UpdateLogo(byte[] logo, string mimeType)
        {
            Logo = logo;
            LogoMimeType = mimeType;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void UpdatePortalColors(
            string sidebarBg,
            string sidebarText,
            string sidebarActiveBg,
            string sidebarActiveText,
            string accentColor,
            string contentBg)
        {
            PortalSidebarBg = sidebarBg ?? PortalSidebarBg;
            PortalSidebarText = sidebarText ?? PortalSidebarText;
            PortalSidebarActiveBg = sidebarActiveBg ?? PortalSidebarActiveBg;
            PortalSidebarActiveText = sidebarActiveText ?? PortalSidebarActiveText;
            PortalAccentColor = accentColor ?? PortalAccentColor;
            PortalContentBg = contentBg ?? PortalContentBg;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void UpdateLandingColors(
            string primaryColor,
            string secondaryColor,
            string accentColor,
            string headerBg,
            string footerBg)
        {
            LandingPrimaryColor = primaryColor ?? LandingPrimaryColor;
            LandingSecondaryColor = secondaryColor ?? LandingSecondaryColor;
            LandingAccentColor = accentColor ?? LandingAccentColor;
            LandingHeaderBg = headerBg ?? LandingHeaderBg;
            LandingFooterBg = footerBg ?? LandingFooterBg;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
