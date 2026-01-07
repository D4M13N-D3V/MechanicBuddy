using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingSocialLink : GuidIdentityEntity
    {
        public virtual string Platform { get; protected set; }
        public virtual string Url { get; protected set; }
        public virtual string DisplayName { get; protected set; }
        public virtual string IconName { get; protected set; }
        public virtual int SortOrder { get; protected set; }
        public virtual bool IsActive { get; protected set; }
        public virtual bool ShowInHeader { get; protected set; }
        public virtual bool ShowInFooter { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected LandingSocialLink() { }

        public LandingSocialLink(
            string platform,
            string url,
            string displayName = null,
            string iconName = null,
            int sortOrder = 0,
            bool showInHeader = true,
            bool showInFooter = true,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            Platform = platform ?? throw new ArgumentNullException(nameof(platform));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            DisplayName = displayName;
            IconName = iconName;
            SortOrder = sortOrder;
            IsActive = true;
            ShowInHeader = showInHeader;
            ShowInFooter = showInFooter;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            string platform,
            string url,
            string displayName,
            string iconName,
            int sortOrder,
            bool isActive,
            bool showInHeader,
            bool showInFooter)
        {
            Platform = platform ?? throw new ArgumentNullException(nameof(platform));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            DisplayName = displayName;
            IconName = iconName;
            SortOrder = sortOrder;
            IsActive = isActive;
            ShowInHeader = showInHeader;
            ShowInFooter = showInFooter;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void UpdateSortOrder(int sortOrder)
        {
            SortOrder = sortOrder;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
