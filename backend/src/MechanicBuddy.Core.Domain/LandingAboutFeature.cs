using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingAboutFeature : GuidIdentityEntity
    {
        public virtual LandingAbout About { get; protected set; }
        public virtual string Text { get; protected set; }
        public virtual int SortOrder { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }

        protected LandingAboutFeature() { }

        public LandingAboutFeature(
            LandingAbout about,
            string text,
            int sortOrder = 0,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            About = about ?? throw new ArgumentNullException(nameof(about));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            SortOrder = sortOrder;
            CreatedAt = DateTime.UtcNow;
        }

        public virtual void Update(string text, int sortOrder)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            SortOrder = sortOrder;
        }
    }
}
