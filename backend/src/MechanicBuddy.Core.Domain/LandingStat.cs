using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingStat : GuidIdentityEntity
    {
        public virtual string Value { get; protected set; }
        public virtual string Label { get; protected set; }
        public virtual int SortOrder { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected LandingStat() { }

        public LandingStat(
            string value,
            string label,
            int sortOrder = 0,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Label = label ?? throw new ArgumentNullException(nameof(label));
            SortOrder = sortOrder;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(string value, string label, int sortOrder)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Label = label ?? throw new ArgumentNullException(nameof(label));
            SortOrder = sortOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void UpdateSortOrder(int sortOrder)
        {
            SortOrder = sortOrder;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
