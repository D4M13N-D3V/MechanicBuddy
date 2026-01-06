using Carmasters.Core.Application.Configuration;
using System;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Services
{
    public class TenantConfigService : ITenantConfigService
    {
        private readonly ITenantConfigRepository repository;

        public TenantConfigService(ITenantConfigRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<RequisitesOptions> GetRequisitesAsync()
        {
            var requisites = await repository.GetRequisitesAsync();

            return new RequisitesOptions(
                requisites.Name,
                requisites.Phone,
                requisites.Address,
                requisites.Email,
                requisites.BankAccount,
                requisites.RegNr,
                requisites.KMKR
            );
        }

        public async Task<PricingOptions> GetPricingAsync()
        {
            var pricing = await repository.GetPricingAsync();

            var invoiceOptions = new InvoiceOptions(
                pricing.VatRate,
                pricing.SurCharge,
                pricing.Disclaimer,
                pricing.SignatureLine,
                pricing.InvoiceEmailContent
            );

            var estimateOptions = new EstimateOptions(
                pricing.EstimateEmailContent
            );

            return new PricingOptions(invoiceOptions, estimateOptions);
        }

        public async Task<AppOptions> GetAppOptionsAsync()
        {
            var requisites = await GetRequisitesAsync();
            var pricing = await GetPricingAsync();

            return new AppOptions(requisites, pricing);
        }

        public async Task SaveRequisitesAsync(RequisitesOptions requisitesOptions)
        {
            var requisites = await repository.GetRequisitesAsync();

            requisites.Update(
                requisitesOptions.Name,
                requisitesOptions.Phone,
                requisitesOptions.Address,
                requisitesOptions.Email,
                requisitesOptions.BankAccount,
                requisitesOptions.RegNr,
                requisitesOptions.KMKR
            );

            await repository.SaveRequisitesAsync(requisites);
        }

        public async Task SavePricingAsync(PricingOptions pricingOptions)
        {
            var pricing = await repository.GetPricingAsync();

            pricing.Update(
                pricingOptions.Invoice.VatRate,
                pricingOptions.Invoice.SurCharge,
                pricingOptions.Invoice.Disclaimer,
                pricingOptions.Invoice.SignatureLine,
                pricingOptions.Invoice.EmailContent,
                pricingOptions.Estimate.EmailContent
            );

            await repository.SavePricingAsync(pricing);
        }

        public async Task SaveAppOptionsAsync(AppOptions appOptions)
        {
            await SaveRequisitesAsync(appOptions.Requisites);
            await SavePricingAsync(appOptions.Pricing);
        }
    }
}