using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingService : GuidIdentityEntity
    {
        public virtual string IconName { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual bool UsePrimaryColor { get; protected set; }
        public virtual int SortOrder { get; protected set; }
        public virtual bool IsActive { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected LandingService() { }

        public LandingService(
            string iconName,
            string title,
            string description,
            bool usePrimaryColor = true,
            int sortOrder = 0,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            IconName = iconName ?? throw new ArgumentNullException(nameof(iconName));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            UsePrimaryColor = usePrimaryColor;
            SortOrder = sortOrder;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            string iconName,
            string title,
            string description,
            bool usePrimaryColor,
            int sortOrder,
            bool isActive)
        {
            IconName = iconName ?? throw new ArgumentNullException(nameof(iconName));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            UsePrimaryColor = usePrimaryColor;
            SortOrder = sortOrder;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void UpdateSortOrder(int sortOrder)
        {
            SortOrder = sortOrder;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
