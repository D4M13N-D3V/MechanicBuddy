using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingGalleryPhoto : GuidIdentityEntity
    {
        public virtual byte[] Image { get; protected set; }
        public virtual string ImageMimeType { get; protected set; }
        public virtual string Caption { get; protected set; }
        public virtual int SortOrder { get; protected set; }
        public virtual bool IsActive { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected LandingGalleryPhoto() { }

        public LandingGalleryPhoto(
            byte[] image,
            string imageMimeType,
            string caption = null,
            int sortOrder = 0,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            Image = image ?? throw new ArgumentNullException(nameof(image));
            ImageMimeType = imageMimeType ?? throw new ArgumentNullException(nameof(imageMimeType));
            Caption = caption;
            SortOrder = sortOrder;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            byte[] image,
            string imageMimeType,
            string caption,
            int sortOrder,
            bool isActive)
        {
            if (image != null && image.Length > 0)
            {
                Image = image;
                ImageMimeType = imageMimeType ?? throw new ArgumentNullException(nameof(imageMimeType));
            }
            Caption = caption;
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
