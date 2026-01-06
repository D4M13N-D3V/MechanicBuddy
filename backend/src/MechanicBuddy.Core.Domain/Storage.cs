using System;

namespace Carmasters.Core.Domain
{
    public class Storage : GuidIdentityEntity
    {
        private string name;
        private string address;

        public  virtual string Name { get => name;}
        public  virtual string Address { get => address;}
        public  virtual string Description { get; protected set; }
        public  virtual DateTime IntroducedAt { get; }

        protected Storage() { }
        public Storage( System.String name, System.String address,string description,DateTime introducedAt)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new UserException("Name is required.");
            this.Id = Guid.Empty;
            this.name = name;
            this.address = address;
            Description = description;
            IntroducedAt = introducedAt;
        }
         
        public  virtual  SparePart AddNewSparePart(string code, string name, decimal price, decimal quantity, short? discount,string description)
        {
            var spare = new SparePart(code, name, price, quantity, discount,description,DateTime.Now);
            spare.StoredAt(this);
            return spare;
        }

        public  virtual void ChangeName(string name)
        {
            this.name = name;
        }
        public  virtual void ChangeAddress(string newAddress)
        {
            this.address = newAddress;
        }

        public  virtual void ChangeDescription(string description)
        {
            this.Description = description;
        }
    }
}