using System;
using System.Collections.Generic;

namespace Carmasters.Core.Domain
{
    public class UmPrice  : GuidIdentityEntity
    {
        private decimal price;
        private string name;
        private string address;
        internal UmPrice() { }
       
        protected internal virtual UmPrice Priced(decimal price, string name, string address) 
        {
            this.price = price;
            this.name = name;
            this.address = address;
            return this;
        }
          
        public  virtual decimal Price { get => price;  }
        public  virtual string Name { get => name;  }
        public  virtual string Address { get => address;  }

        
    }
}