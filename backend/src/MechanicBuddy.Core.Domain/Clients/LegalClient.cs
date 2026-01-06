using System;
using System.Collections.Generic;

namespace Carmasters.Core.Domain
{
    public class LegalClient : Client
    {
        string name;
        protected LegalClient():base() { }
        public LegalClient(DateTime introducedAt, string name, string regNr ,  AddressComponent address = null, string phone = null,
            bool isAsshole = false, string description =null, Guid? id = null) : base(address, phone, isAsshole, description,introducedAt, id)
        {
            SetNameAndRegNr(name, regNr);
        }

        private void SetNameAndRegNr(string name, string regNr)
        {
            this.name = name ?? throw new UserException("Name is required.");
            RegNr = regNr ?? throw new UserException("Registration nr is required.");
        }
         
        public  override string Name { get => name; }
        public override string RegCode => RegNr;
        public  virtual string RegNr { get;  protected set; }

        public  virtual void Edit(string name, string regNr, AddressComponent address, string phone, bool isAsshole,string notes)
        {
            base.SetContact(address, phone, isAsshole, notes,this.IntroducedAt);
            SetNameAndRegNr(name, regNr);
        }
    }
}