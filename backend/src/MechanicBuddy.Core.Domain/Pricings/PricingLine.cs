using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmasters.Core.Domain
{
    public class PricingLine  
    {
        private readonly Pricing pricing;

        protected PricingLine() { }
        public  PricingLine(Pricing pricing,
                           short nr,
                           string description,
                           decimal? quantity,
                           decimal unitPrice,
                           string unit,
                           decimal? discount,
                           decimal total,
                           decimal totalWithVat)
        {
            if (pricing is null)
            {
                throw new ArgumentNullException(nameof(pricing));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException($"'{nameof(description)}' cannot be null or empty", nameof(description));
            }

            
            this.pricing = pricing;
            Nr = nr;
            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Unit = unit;
            Discount = discount;
            Total = total;
            TotalWithVat = totalWithVat;
        }


        internal static void Synchronize(PricingLine[] newSet, IList<PricingLine> currentSet)
        {
            foreach (var update in newSet)
            {
                var existingProduct = currentSet.SingleOrDefault(x => x.Nr == update.Nr);
                if (existingProduct == null)
                {
                    currentSet.Add(update);
                }
                else
                {
                    existingProduct.Nr = update.Nr;
                    existingProduct.Description = update.Description; 
                    existingProduct.Quantity = update.Quantity;
                    existingProduct.Unit = update.Unit;
                    existingProduct.UnitPrice = update.UnitPrice;
                    existingProduct.Discount = update.Discount;
                    existingProduct.Total = update.Total;
                    existingProduct.TotalWithVat = update.TotalWithVat;
                }
            }
            var toRemove = currentSet.Where(p => !newSet.Any(x => x.Nr == p.Nr)).ToList();
            foreach (var deleteProduct in toRemove)
            {
                currentSet.Remove(deleteProduct);
            }
        }
        public  virtual short Nr { get; protected set; }
        public  virtual string Description { get; protected set; }
        public  virtual decimal? Quantity { get; protected set; }
        public  virtual decimal UnitPrice { get; protected set; }
        public  virtual string Unit { get; protected set; }
        public  virtual decimal? Discount { get; protected set; }
        public  virtual decimal Total { get; protected set; }
        public  virtual decimal TotalWithVat { get; protected set; }

        public override bool Equals(object obj)
        {
            return obj is PricingLine line &&
                   pricing == line.pricing &&
                   Nr == line.Nr;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(pricing, Nr);
        }
    }
}