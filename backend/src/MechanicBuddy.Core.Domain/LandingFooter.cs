using System;

namespace MechanicBuddy.Core.Domain
{
    public class LandingFooter : GuidIdentityEntity
    {
        public virtual string CompanyDescription { get; protected set; }
        public virtual bool ShowQuickLinks { get; protected set; }
        public virtual bool ShowContactInfo { get; protected set; }
        public virtual string CopyrightText { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected LandingFooter() { }

        public LandingFooter(
            string companyDescription = null,
            bool showQuickLinks = true,
            bool showContactInfo = true,
            string copyrightText = null,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            CompanyDescription = companyDescription;
            ShowQuickLinks = showQuickLinks;
            ShowContactInfo = showContactInfo;
            CopyrightText = copyrightText;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            string companyDescription,
            bool showQuickLinks,
            bool showContactInfo,
            string copyrightText)
        {
            CompanyDescription = companyDescription;
            ShowQuickLinks = showQuickLinks;
            ShowContactInfo = showContactInfo;
            CopyrightText = copyrightText;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
