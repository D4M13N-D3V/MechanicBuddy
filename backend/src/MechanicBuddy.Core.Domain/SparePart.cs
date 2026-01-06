using System;

namespace Carmasters.Core.Domain
{
    public class SparePart : GuidIdentityEntity
    {
        public  virtual string Description { get; protected set; }
        public  virtual DateTime IntroducedAt { get;  }

        private string code;
        private string name;
        private decimal price;
        private decimal quantity;
        private short? discount;
        private Storage storage;

        public  virtual string Code { get => code; }
        public  virtual string Name { get => name;}

        public virtual void PriceFromUm(decimal price, string name, string address)
        {
            if (this.UmPrice == null) this.UmPrice = new UmPrice();
            this.UmPrice.Priced(price, name, address);
            this.price = price;
        }

        public  virtual decimal Price { get => price;}
        public  virtual decimal Quantity { get => quantity;}
        public  virtual short? Discount { get => discount; }
        public  virtual Storage Storage { get => storage; }

        public virtual UmPrice UmPrice { get; protected set; }

        protected SparePart() { }
        public  SparePart(string code, string name, decimal price, decimal quantity, short? discount, string description, DateTime introducedAt, Storage storage=null, Guid? id = null)
        { 
            this.storage = storage;
            this.Id = id.GetValueOrDefault(); 
            this.IntroducedAt = introducedAt;
            SetValues(code, name, price, quantity, discount,description); 
        }
         
        public  virtual void StoredAt(Storage storage)
        {
            this.storage = storage ?? throw new System.ArgumentNullException(nameof(storage));
            this.storage = storage;
        }

        
        private void SetValues(string code, string name, decimal price, decimal quantity, short? discount, string description)
        {
            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name)) throw new UserException("Either name or code is required.");
            this.Description = description;
            this.code = code;
            this.name = name;
            this.price = price;
            this.quantity = quantity;
            this.discount = discount; 
        }
         
        public  virtual void Edit(string code, string name, decimal price, decimal quantity, short? discount,string description)
        {
            this.SetValues(code, name, price, quantity, discount,description);
        }
         
    }
}