using MechanicBuddy.Core.Application.Services;
using MechanicBuddy.Core.Domain;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MechanicBuddy.Core.Persistence.Postgres.Repositories
{
    public class BrandingRepository : IBrandingRepository
    {
        private readonly ISession session;

        public BrandingRepository(ISession session)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        // Branding
        public async Task<TenantBranding> GetBrandingAsync()
        {
            var branding = session.QueryOver<TenantBranding>().List<TenantBranding>().FirstOrDefault();

            if (branding == null)
            {
                branding = new TenantBranding();
                await session.SaveAsync(branding);
                await session.FlushAsync();
            }

            return branding;
        }

        public async Task SaveBrandingAsync(TenantBranding branding)
        {
            if (branding == null)
                throw new ArgumentNullException(nameof(branding));

            await session.SaveOrUpdateAsync(branding);
            await session.FlushAsync();
        }

        // Hero
        public async Task<LandingHero> GetHeroAsync()
        {
            var hero = session.QueryOver<LandingHero>().List<LandingHero>().FirstOrDefault();

            if (hero == null)
            {
                hero = new LandingHero("Your Auto Shop", "Professional Auto Repair & Maintenance");
                await session.SaveAsync(hero);
                await session.FlushAsync();
            }

            return hero;
        }

        public async Task SaveHeroAsync(LandingHero hero)
        {
            if (hero == null)
                throw new ArgumentNullException(nameof(hero));

            await session.SaveOrUpdateAsync(hero);
            await session.FlushAsync();
        }

        // Services
        public async Task<IList<LandingService>> GetServicesAsync()
        {
            return await session.Query<LandingService>()
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        public async Task<LandingService> GetServiceByIdAsync(Guid id)
        {
            return await session.GetAsync<LandingService>(id);
        }

        public async Task SaveServiceAsync(LandingService service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            await session.SaveOrUpdateAsync(service);
            await session.FlushAsync();
        }

        public async Task DeleteServiceAsync(Guid id)
        {
            var service = await session.GetAsync<LandingService>(id);
            if (service != null)
            {
                await session.DeleteAsync(service);
                await session.FlushAsync();
            }
        }

        // About
        public async Task<LandingAbout> GetAboutAsync()
        {
            var about = session.QueryOver<LandingAbout>().List<LandingAbout>().FirstOrDefault();

            if (about == null)
            {
                about = new LandingAbout("Your Trusted Auto Repair Experts");
                await session.SaveAsync(about);
                await session.FlushAsync();
            }

            return about;
        }

        public async Task SaveAboutAsync(LandingAbout about)
        {
            if (about == null)
                throw new ArgumentNullException(nameof(about));

            await session.SaveOrUpdateAsync(about);
            await session.FlushAsync();
        }

        public async Task<LandingAboutFeature> GetAboutFeatureByIdAsync(Guid id)
        {
            return await session.GetAsync<LandingAboutFeature>(id);
        }

        public async Task SaveAboutFeatureAsync(LandingAboutFeature feature)
        {
            if (feature == null)
                throw new ArgumentNullException(nameof(feature));

            await session.SaveOrUpdateAsync(feature);
            await session.FlushAsync();
        }

        public async Task DeleteAboutFeatureAsync(Guid id)
        {
            var feature = await session.GetAsync<LandingAboutFeature>(id);
            if (feature != null)
            {
                await session.DeleteAsync(feature);
                await session.FlushAsync();
            }
        }

        // Stats
        public async Task<IList<LandingStat>> GetStatsAsync()
        {
            return await session.Query<LandingStat>()
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        public async Task<LandingStat> GetStatByIdAsync(Guid id)
        {
            return await session.GetAsync<LandingStat>(id);
        }

        public async Task SaveStatAsync(LandingStat stat)
        {
            if (stat == null)
                throw new ArgumentNullException(nameof(stat));

            await session.SaveOrUpdateAsync(stat);
            await session.FlushAsync();
        }

        public async Task DeleteStatAsync(Guid id)
        {
            var stat = await session.GetAsync<LandingStat>(id);
            if (stat != null)
            {
                await session.DeleteAsync(stat);
                await session.FlushAsync();
            }
        }

        // Tips Section
        public async Task<LandingTipsSection> GetTipsSectionAsync()
        {
            var tipsSection = session.QueryOver<LandingTipsSection>().List<LandingTipsSection>().FirstOrDefault();

            if (tipsSection == null)
            {
                tipsSection = new LandingTipsSection();
                await session.SaveAsync(tipsSection);
                await session.FlushAsync();
            }

            return tipsSection;
        }

        public async Task SaveTipsSectionAsync(LandingTipsSection tipsSection)
        {
            if (tipsSection == null)
                throw new ArgumentNullException(nameof(tipsSection));

            await session.SaveOrUpdateAsync(tipsSection);
            await session.FlushAsync();
        }

        // Tips
        public async Task<IList<LandingTip>> GetTipsAsync()
        {
            return await session.Query<LandingTip>()
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
        }

        public async Task<LandingTip> GetTipByIdAsync(Guid id)
        {
            return await session.GetAsync<LandingTip>(id);
        }

        public async Task SaveTipAsync(LandingTip tip)
        {
            if (tip == null)
                throw new ArgumentNullException(nameof(tip));

            await session.SaveOrUpdateAsync(tip);
            await session.FlushAsync();
        }

        public async Task DeleteTipAsync(Guid id)
        {
            var tip = await session.GetAsync<LandingTip>(id);
            if (tip != null)
            {
                await session.DeleteAsync(tip);
                await session.FlushAsync();
            }
        }

        // Footer
        public async Task<LandingFooter> GetFooterAsync()
        {
            var footer = session.QueryOver<LandingFooter>().List<LandingFooter>().FirstOrDefault();

            if (footer == null)
            {
                footer = new LandingFooter();
                await session.SaveAsync(footer);
                await session.FlushAsync();
            }

            return footer;
        }

        public async Task SaveFooterAsync(LandingFooter footer)
        {
            if (footer == null)
                throw new ArgumentNullException(nameof(footer));

            await session.SaveOrUpdateAsync(footer);
            await session.FlushAsync();
        }

        // Contact
        public async Task<LandingContact> GetContactAsync()
        {
            var contact = session.QueryOver<LandingContact>().List<LandingContact>().FirstOrDefault();

            if (contact == null)
            {
                contact = new LandingContact();
                await session.SaveAsync(contact);
                await session.FlushAsync();
            }

            return contact;
        }

        public async Task SaveContactAsync(LandingContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            await session.SaveOrUpdateAsync(contact);
            await session.FlushAsync();
        }
    }
}
