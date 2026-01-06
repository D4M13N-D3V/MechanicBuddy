using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmasters.Core.Domain
{
    public class PriceSummary
    {
        public decimal TotalWithVat { get; protected set; }
        public decimal TotalWithoutVat { get; protected set; } 
        private PriceSummary() { }

 
        private PriceSummary(decimal price,decimal quantity,decimal discount, decimal taxMultiplier)
        { 
            if (quantity < 1)
            {
                TotalWithVat = 0;
                TotalWithoutVat = 0;
            }
            
            var bargainMultiplier = ((100 + discount ) / (decimal)100);
            TotalWithVat = quantity * price * bargainMultiplier;
            TotalWithoutVat = (quantity * (price / taxMultiplier)) * bargainMultiplier;
        }

        public PriceSummary(Saleable saleable,decimal taxMultiplier) : this(saleable.Price, saleable.Quantity.GetValueOrDefault(), saleable.Discount.GetValueOrDefault(), taxMultiplier)
        {

        }  

        public static PriceSummary CalculatePriceSummary(int purchaseTax,IEnumerable<Saleable> saleables) 
        {
            var lines = saleables.Select(x => new PriceSummary(x, Pricing.TaxMultiplier(purchaseTax)));
            return new PriceSummary {
                TotalWithVat = lines.Sum((item) => item.TotalWithVat),
                TotalWithoutVat = lines.Sum((item) => item.TotalWithoutVat)
                };
        }
    } 
}