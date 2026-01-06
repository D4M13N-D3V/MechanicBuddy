using System;

namespace Carmasters.Core.Domain
{
    public class TenantRequisites : GuidIdentityEntity
    {
        public virtual string Name { get; protected set; }
        public virtual string Phone { get; protected set; }
        public virtual string Address { get; protected set; }
        public virtual string Email { get; protected set; }
        public virtual string BankAccount { get; protected set; }
        public virtual string RegNr { get; protected set; }
        public virtual string KMKR { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual DateTime UpdatedAt { get; protected set; }

        protected TenantRequisites() { }

        public TenantRequisites(
            string name,
            string phone,
            string address,
            string email,
            string bankAccount,
            string regNr,
            string kmkr,
            Guid? id = null)
        {
            Id = id.GetValueOrDefault();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Phone = phone;
            Address = address;
            Email = email;
            BankAccount = bankAccount;
            RegNr = regNr;
            KMKR = kmkr;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Update(
            string name,
            string phone,
            string address,
            string email,
            string bankAccount,
            string regNr,
            string kmkr)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Phone = phone;
            Address = address;
            Email = email;
            BankAccount = bankAccount;
            RegNr = regNr;
            KMKR = kmkr;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}