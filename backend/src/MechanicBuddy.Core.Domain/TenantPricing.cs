using System;

namespace Carmasters.Core.Domain
{

    public class TenantPricing : GuidIdentityEntity
    {
        public virtual int VatRate { get; protected set; }
        public virtual string SurCharge { get; protected set; }
        public virtual string Disclaimer { get; protected set; }
        public virtual bool SignatureLine { get; protected set; }
        public virtual string InvoiceEmailContent { get; protected set; }
        public virtual string EstimateEmailContent { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected TenantPricing() { }

        public TenantPricing(
            int vatRate,
            string surCharge,
            string disclaimer,
            bool signatureLine,
            string invoiceEmailContent,
            string estimateEmailContent,
            Guid? id = null)
        {
            Id = id.GetValueOrDefault();
            VatRate = vatRate;
            SurCharge = surCharge;
            Disclaimer = disclaimer;
            SignatureLine = signatureLine;
            InvoiceEmailContent = invoiceEmailContent;
            EstimateEmailContent = estimateEmailContent;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            int vatRate,
            string surCharge,
            string disclaimer,
            bool signatureLine,
            string invoiceEmailContent,
            string estimateEmailContent)
        {
            VatRate = vatRate;
            SurCharge = surCharge;
            Disclaimer = disclaimer;
            SignatureLine = signatureLine;
            InvoiceEmailContent = invoiceEmailContent;
            EstimateEmailContent = estimateEmailContent;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}