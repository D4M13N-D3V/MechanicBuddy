using MechanicBuddy.Core.Application.Configuration;
using MechanicBuddy.Core.Domain;

namespace MechanicBuddy.Core.Application.Model
{
    public class PricingPrintModel
    {
        public Pricing Pricing { get; set; }
        public RequisitesOptions RequisitesOptions { get; set; }
        public PricingOptions PricingOptions { get; set; }
    }
}
