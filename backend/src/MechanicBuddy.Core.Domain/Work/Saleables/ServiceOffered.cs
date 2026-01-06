using System;

namespace Carmasters.Core.Domain
{
    public class ServiceOffered : Saleable
    {
        private readonly Offer offer;
        protected ServiceOffered() : base() { }
        public  ServiceOffered(Offer offer,string name, decimal quantity, string unit, decimal price, short? discount = null, Guid? id = null) : base(name, quantity, unit, price, discount, id)
        {
            this.offer = offer;
        }

    }
}