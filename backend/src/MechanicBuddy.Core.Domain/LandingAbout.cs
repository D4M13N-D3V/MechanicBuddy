using System;
using System.Collections.Generic;

namespace MechanicBuddy.Core.Domain
{
    public class LandingAbout : GuidIdentityEntity
    {
        public virtual string SectionLabel { get; protected set; }
        public virtual string Headline { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual string SecondaryDescription { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        public virtual IList<LandingAboutFeature> Features { get; protected set; }

        protected LandingAbout()
        {
            Features = new List<LandingAboutFeature>();
        }

        public LandingAbout(
            string headline,
            string description = null,
            string secondaryDescription = null,
            string sectionLabel = "About Us",
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            Headline = headline ?? throw new ArgumentNullException(nameof(headline));
            Description = description;
            SecondaryDescription = secondaryDescription;
            SectionLabel = sectionLabel ?? "About Us";
            Features = new List<LandingAboutFeature>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            string headline,
            string description,
            string secondaryDescription,
            string sectionLabel)
        {
            Headline = headline ?? throw new ArgumentNullException(nameof(headline));
            Description = description;
            SecondaryDescription = secondaryDescription;
            SectionLabel = sectionLabel ?? SectionLabel;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual LandingAboutFeature AddFeature(string text, int sortOrder)
        {
            var feature = new LandingAboutFeature(this, text, sortOrder);
            Features.Add(feature);
            UpdatedAt = DateTime.UtcNow;
            return feature;
        }

        public virtual void RemoveFeature(LandingAboutFeature feature)
        {
            Features.Remove(feature);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
