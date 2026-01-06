using System;

namespace Carmasters.Core.Domain
{
    public class ProductOffered : Product
    {
        public virtual Offer Offer { get; }
        protected ProductOffered() : base() { }
        public  ProductOffered(Offer offer,
                              short jnr,
                              string code,
                              string name,
                              decimal? quantity,
                              string unit,
                              decimal price,
                              short? discount = null,
                             Guid? id = null) : base(jnr,code,name, quantity, unit, price, discount, id)
        {
            
            this.Offer = offer; 
        } 
        public  virtual ServiceOffered Service { get; protected set; } 
        public  virtual void WithIn(ServiceOffered service)
        {
            this.Service = service;
        }

        protected internal virtual ProductOffered MakeCopy(Offer offer)
        {
           return new ProductOffered(offer, Jnr,Code,Name,Quantity,Unit,Price,Discount);
        }
    } 
}