using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmasters.Core.Domain
{
    public abstract class Product : Saleable
    { 
        protected Product() : base() { }
        protected Product(short jnr,
                                 string code,
                                string name,
                                decimal? quantity,
                                string unit,
                                decimal price,
                                short? discount = null,
                                Guid? id = null) : base(name, quantity, unit, price, discount, id)
        {
             
            Code = code;
            Jnr = jnr;
        }

        public virtual short Jnr { get; protected set; }
        public virtual string Code { get; protected set; } 
         
        
        internal static void Synchronize<T>(T[] newSet, IList<T> currentSet) where T : Product
        {
            foreach (var update in newSet)
            {
                var existingProduct = currentSet.SingleOrDefault(x => x == update);
                if (existingProduct == null)
                {
                    currentSet.Add(update);
                }
                else
                {  
                    existingProduct.Code = update.Code;
                    existingProduct.Jnr = update.Jnr;
                    existingProduct.Name = update.Name;
                    existingProduct.Quantity = update.Quantity;
                    existingProduct.Unit = update.Unit;
                    existingProduct.Price = update.Price;
                    existingProduct.Discount = update.Discount;
                }
            }
            var toRemove = currentSet.Where(p => !newSet.Any(x => x == p)).ToList();
            foreach (var deleteProduct in toRemove)
            {
                currentSet.Remove(deleteProduct);
            }
        }
    }
}