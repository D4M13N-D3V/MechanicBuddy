using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingHero : GuidIdentityEntity
    {
        public virtual string CompanyName { get; protected set; }
        public virtual string Tagline { get; protected set; }
        public virtual string Subtitle { get; protected set; }
        public virtual string SpecialtyText { get; protected set; }
        public virtual string CtaPrimaryText { get; protected set; }
        public virtual string CtaPrimaryLink { get; protected set; }
        public virtual string CtaSecondaryText { get; protected set; }
        public virtual string CtaSecondaryLink { get; protected set; }
        public virtual byte[] BackgroundImage { get; protected set; }
        public virtual string BackgroundImageMimeType { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected LandingHero() { }

        public LandingHero(
            string companyName,
            string tagline = null,
            string subtitle = null,
            string specialtyText = null,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            CompanyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
            Tagline = tagline;
            Subtitle = subtitle;
            SpecialtyText = specialtyText;
            CtaPrimaryText = "Our Services";
            CtaPrimaryLink = "#services";
            CtaSecondaryText = "Contact Us";
            CtaSecondaryLink = "#contact";
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            string companyName,
            string tagline,
            string subtitle,
            string specialtyText,
            string ctaPrimaryText,
            string ctaPrimaryLink,
            string ctaSecondaryText,
            string ctaSecondaryLink)
        {
            CompanyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
            Tagline = tagline;
            Subtitle = subtitle;
            SpecialtyText = specialtyText;
            CtaPrimaryText = ctaPrimaryText ?? CtaPrimaryText;
            CtaPrimaryLink = ctaPrimaryLink ?? CtaPrimaryLink;
            CtaSecondaryText = ctaSecondaryText ?? CtaSecondaryText;
            CtaSecondaryLink = ctaSecondaryLink ?? CtaSecondaryLink;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void UpdateBackgroundImage(byte[] image, string mimeType)
        {
            BackgroundImage = image;
            BackgroundImageMimeType = mimeType;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
