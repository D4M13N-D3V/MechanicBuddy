using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingTipsSection : GuidIdentityEntity
    {
        public virtual bool IsVisible { get; protected set; }
        public virtual string SectionLabel { get; protected set; }
        public virtual string Headline { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected LandingTipsSection() { }

        public LandingTipsSection(
            string headline = "Auto Care Tips",
            string sectionLabel = "Expert Advice",
            string description = null,
            bool isVisible = true,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            IsVisible = isVisible;
            SectionLabel = sectionLabel ?? "Expert Advice";
            Headline = headline ?? "Auto Care Tips";
            Description = description;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            bool isVisible,
            string sectionLabel,
            string headline,
            string description)
        {
            IsVisible = isVisible;
            SectionLabel = sectionLabel ?? SectionLabel;
            Headline = headline ?? Headline;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
