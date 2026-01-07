using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingTip : GuidIdentityEntity
    {
        public virtual string Title { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual int SortOrder { get; protected set; }
        public virtual bool IsActive { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected LandingTip() { }

        public LandingTip(
            string title,
            string description,
            int sortOrder = 0,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            SortOrder = sortOrder;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            string title,
            string description,
            int sortOrder,
            bool isActive)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
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
