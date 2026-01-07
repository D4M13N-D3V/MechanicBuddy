using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingSectionVisibility : GuidIdentityEntity
    {
        public virtual bool HeroVisible { get; protected set; }
        public virtual bool ServicesVisible { get; protected set; }
        public virtual bool AboutVisible { get; protected set; }
        public virtual bool StatsVisible { get; protected set; }
        public virtual bool TipsVisible { get; protected set; }
        public virtual bool GalleryVisible { get; protected set; }
        public virtual bool ContactVisible { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected LandingSectionVisibility() { }

        public LandingSectionVisibility(
            bool heroVisible = true,
            bool servicesVisible = true,
            bool aboutVisible = true,
            bool statsVisible = true,
            bool tipsVisible = true,
            bool galleryVisible = true,
            bool contactVisible = true,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            HeroVisible = heroVisible;
            ServicesVisible = servicesVisible;
            AboutVisible = aboutVisible;
            StatsVisible = statsVisible;
            TipsVisible = tipsVisible;
            GalleryVisible = galleryVisible;
            ContactVisible = contactVisible;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            bool heroVisible,
            bool servicesVisible,
            bool aboutVisible,
            bool statsVisible,
            bool tipsVisible,
            bool galleryVisible,
            bool contactVisible)
        {
            HeroVisible = heroVisible;
            ServicesVisible = servicesVisible;
            AboutVisible = aboutVisible;
            StatsVisible = statsVisible;
            TipsVisible = tipsVisible;
            GalleryVisible = galleryVisible;
            ContactVisible = contactVisible;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
