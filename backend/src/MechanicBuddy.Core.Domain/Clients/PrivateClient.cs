using System;

namespace Carmasters.Core.Domain
{
    public class PrivateClient : Client
    {
        protected PrivateClient() : base() { }
        public PrivateClient(DateTime introducedAt,
                             string firstName,
                             string lastName = null,
                             AddressComponent address = null,
                             string phone = null,
                             bool isAsshole = false,
                             string description = null,
                             string personalCode = null,
                            Guid? id = null) : base(address, phone, isAsshole,description,introducedAt, id)
        {
            SetName(firstName, lastName);
            this.PersonalCode = personalCode;
        }

        private void SetName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw  new UserException("Firstname is required.");
            }

            FirstName = firstName;
            LastName = lastName;
        }

        public  virtual string FirstName { get; protected set; }
        public  virtual string LastName { get; protected set; }

        public virtual string PersonalCode { get; protected set; }
        public override string RegCode => PersonalCode;
        public override string Name => string.Concat(FirstName," ",LastName);
        public  virtual void Edit(string firstName, string lastName, AddressComponent address, string phone, bool isAsshole,string notes,string personaCode)
        {
            base.SetContact(address, phone,  isAsshole,notes,this.IntroducedAt);
            this.SetName(firstName, lastName);
            this.PersonalCode = personaCode;
        }
    }
}