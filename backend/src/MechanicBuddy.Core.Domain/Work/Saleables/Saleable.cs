using System;

namespace Carmasters.Core.Domain
{
    public abstract class Saleable : GuidIdentityEntity
    {
        protected Saleable() { }
        protected Saleable(string name, decimal? quantity, string unit, decimal price, short? discount = null, Guid? id = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new UserException("Name required."); 

            Name = name;
            Quantity = quantity <= 0 ? throw new UserException("Quantity must be at least 1.") : quantity;
            Unit = unit;
            Price = price; 
            Discount = discount;
            Id = id.GetValueOrDefault();
        }

        public virtual string Name { get; protected set; }
        public virtual decimal? Quantity { get; protected set; }
        public virtual string Unit { get; protected set; }
        public virtual decimal Price { get; protected set; }
        public virtual short? Discount { get; protected set; } 
    }
}