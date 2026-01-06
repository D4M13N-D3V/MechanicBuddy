using System;
using System.Collections.Generic;

namespace Carmasters.Core.Domain
{
    public    class ClientEmail 
    {
        readonly Client client;

        protected ClientEmail() { }
        internal    ClientEmail(Client client, string address, bool isActive = true) {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException($"'{nameof(address)}' cannot be null or whitespace", nameof(address));
            }
            this.client = client;
            Address = address;
            IsActive = isActive;
        }

        public virtual void InUse()
        {
            foreach (var item in client.EmailAddresses)
            {
                item.IsActive = this == item;
            }
        }

        public  virtual string Address { get; }
        public  virtual bool IsActive { get; protected set; }

        public  override bool Equals(object obj)
        {
            return obj is ClientEmail email &&
                   client == email.client &&
                   Address == email.Address;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(client, Address);
        }

      
    }
}