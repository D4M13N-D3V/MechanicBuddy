using Carmasters.Core.Application.Services;
using Carmasters.Core.Domain;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Carmasters.Core.Persistence.Postgres.Repositories
{
    public class TenantConfigRepository : ITenantConfigRepository
    {
        private readonly ISession session;

        public TenantConfigRepository(ISession session)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<TenantRequisites> GetRequisitesAsync()
        {
            // Get the first record (should only be one per tenant)
            var requisites = session.QueryOver<TenantRequisites>().List<TenantRequisites>().Single();
               

            if (requisites == null)
            {
                // Create default if none exists
                requisites = new TenantRequisites(
                    "Default Company",
                    "+1234567890",
                    "123 Main St",
                    "info@example.com",
                    "EE123456789012",
                    "REG12345",
                    "KMKR123456"
                );
                await session.SaveAsync(requisites);
                await session.FlushAsync();
            }
            await Task.CompletedTask;
            return requisites;
        }

        public async Task<TenantPricing> GetPricingAsync()
        {
            // Get the first record (should only be one per tenant)
            var pricing =   session.QueryOver<TenantPricing>().List<TenantPricing>().Single();
                

            if (pricing == null)
            {
                // Create default if none exists
                pricing = new TenantPricing(
                    20,
                    "Default Surcharge",
                    "Default Disclaimer",
                    true,
                    "Thank you for your business. Please find your invoice attached.",
                    "Thank you for your interest. Please find your estimate attached."
                );
                await session.SaveAsync(pricing);
                await session.FlushAsync();
            }
            await Task.CompletedTask;
            return pricing;
        }

        public async Task SaveRequisitesAsync(TenantRequisites requisites)
        {
            if (requisites == null)
                throw new ArgumentNullException(nameof(requisites));

            await session.SaveOrUpdateAsync(requisites);
            await session.FlushAsync();
        }

        public async Task SavePricingAsync(TenantPricing pricing)
        {
            if (pricing == null)
                throw new ArgumentNullException(nameof(pricing));

            await session.SaveOrUpdateAsync(pricing);
            await session.FlushAsync();
        }
    }
}