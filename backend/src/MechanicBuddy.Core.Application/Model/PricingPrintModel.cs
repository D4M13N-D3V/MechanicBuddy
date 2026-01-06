using Carmasters.Core.Application.Configuration;
using Carmasters.Core.Domain;

namespace Carmasters.Core.Application.Model
{
    public class PricingPrintModel
    {
        public Pricing Pricing { get; set; }
        public RequisitesOptions RequisitesOptions { get; set; }
        public PricingOptions PricingOptions { get; set; }
    }
}
