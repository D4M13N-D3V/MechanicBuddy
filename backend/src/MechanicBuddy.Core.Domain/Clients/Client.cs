using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;

namespace System
{
    public static class Extensions 
    {
        public static bool IsIn<T>(this T value, T[] list) 
        {
            return list.Contains(value);
        }
        public static void RemoveWhere<TEntity>(this IList<TEntity> entities, Func<TEntity, bool> predicate) where TEntity : class
        {
            var records = entities
                .Where(predicate)
                .ToList();
            if (records.Count > 0)
            {
                foreach (var item in records)
                {
                    entities.Remove(item);
                }
            }
        }
    }
}

namespace Carmasters.Core.Domain
{

    public    abstract class Client : GuidIdentityEntity
    {
        IList<ClientEmail> emailAddresses = new List<ClientEmail>();
        protected Client() { }
        protected Client(AddressComponent address, string phone, bool isAsshole, string notes, DateTime introducedAt, Guid? id = null)
        {
            this.Id = id.GetValueOrDefault();
            SetContact(address, phone, isAsshole, notes, introducedAt);
        }
       
        protected void SetContact(AddressComponent address, string phone, bool isAsshole, string notes, DateTime createdat)
        {
            Address = address;
            Phone = phone; 
            IsAsshole = isAsshole;
            IntroducedAt = createdat;
            Description = notes;
        }
        public  virtual IReadOnlyCollection<ClientEmail> EmailAddresses => emailAddresses.ToList().AsReadOnly();
        public  virtual DateTime IntroducedAt { get; protected set; }
        public  virtual string Description { get; protected set; }
        public virtual AddressComponent Address { get; protected set; }
        public  virtual string Phone { get; protected set; }

        public virtual void UsesEmail(string address) 
        {
            this.UsesEmail(new[] { address }, address);
        }
       
        public  virtual void UsesEmail(string[] addresses,string activeAddress = null) 
        {
            if (addresses?.Any() == false) return;
            this.emailAddresses.
                RemoveWhere(x => !x.Address.IsIn(addresses));
             
            foreach (var item in addresses)
            {
                var exists = this.emailAddresses.Any(x => x.Address == item);
                if(!exists) this.emailAddresses.Add(new ClientEmail(this, item,false));
            }

            if (activeAddress != null) 
            {
                ChangeCurrentEmail(activeAddress);
            }

            if (emailAddresses.Count(x => x.IsActive) > 1) throw new Exception("only 1 active email allowed");
        }
           
        public  virtual string CurrentEmail { get { return emailAddresses.SingleOrDefault(x => x.IsActive)?.Address; }}
        public  virtual bool IsAsshole { get; protected set; }

        public  abstract string Name { get; }
        public abstract string RegCode { get; }
        public  virtual void ChangeCurrentEmail(string email)
        {
            emailAddresses.
                Single(x => x.Address == email).
                InUse(); 
        }
    }
}