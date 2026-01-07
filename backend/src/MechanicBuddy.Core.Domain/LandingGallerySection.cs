using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingGallerySection : GuidIdentityEntity
    {
        public virtual string SectionLabel { get; protected set; }
        public virtual string Headline { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected LandingGallerySection() { }

        public LandingGallerySection(
            string headline = "Photo Gallery",
            string sectionLabel = "Our Work",
            string description = null,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            SectionLabel = sectionLabel ?? "Our Work";
            Headline = headline ?? "Photo Gallery";
            Description = description;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            string sectionLabel,
            string headline,
            string description)
        {
            SectionLabel = sectionLabel ?? SectionLabel;
            Headline = headline ?? Headline;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
