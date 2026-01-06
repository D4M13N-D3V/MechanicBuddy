
using Carmasters.Core.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Carmasters.Core.Domain
{
    public abstract partial class Pricing : GuidIdentityEntity
    {
        protected Pricing() { }

         
        protected Pricing ApplyVehicleInformation(Vehicle vehicle) 
        {
            if (vehicle == null) 
            {
                this.VehicleLine1 = this.VehicleLine2 = this.VehicleLine3 = this.VehicleLine4 = String.Empty;
                return this;
            }
            this.VehicleLine1 = "Sõiduk: " + vehicle.Producer + " " + vehicle.Model;
            this.VehicleLine2 =  "Reg nr: " + vehicle.RegNr;
            this.VehicleLine3 = "Odomeetri näit: " + vehicle.Odo;
            this.VehicleLine4 = "VIN: " + vehicle.Vin;
            return this;
        }

        protected Pricing ApplyClientInformation(Client client) 
        {
            this.Email = client?.CurrentEmail;
            this.PartyName = client == null ? "eraisik" : client.Name;
            this.PartyAddress = client?.Address?.ToString();
             this.PartyCode = client?.RegCode;
            return this;
        }
        protected Pricing( Employee issuer,DateTime? sentOn, DateTime? printedOn, string email, string partyName, string partyAddress, string partyCode, string vehicleLine1, string vehicleLine2, string vehicleLine3,string vehicleLine4, DateTime issuedOn, Guid? id = null)
        {
             
            this.Issuer = issuer;
            Id = id.GetValueOrDefault();
            SentOn = sentOn;
            PrintedOn = printedOn;
            Email = email;
            PartyName = partyName;
            PartyAddress = partyAddress;
            PartyCode = partyCode;
            VehicleLine1 = vehicleLine1;
            VehicleLine2 = vehicleLine2;
            VehicleLine3 = vehicleLine3;
            VehicleLine4 = vehicleLine4;
            IssuedOn = issuedOn;
           
        }

        public abstract string GetFileName();
        public abstract string GetDisplayName();


        public virtual async Task Send(IPricingSender sender, string receipient)
        { 
            this.Email = receipient;
            this.SentOn = DateTime.Now;
            await sender.Send(this);
        }

        public virtual decimal GetTotal(bool withVat)
        {
            return this.Lines.Sum(x => withVat ? x.TotalWithVat : x.Total);
        }
        protected IList<PricingLine> lines = new List<PricingLine>();
        public  virtual IEnumerable<PricingLine> Lines => lines.ToList();
         
        public  virtual void AddLine(int purchaseTax,Saleable saleable)
        {
            lines.Add(ToLine(purchaseTax,saleable, Convert.ToInt16(lines.Count() + 1)));
        }
         
        internal static decimal TaxMultiplier(int purchaseTax) =>  (1 + purchaseTax / (decimal)100)  ;
        
        protected PricingLine ToLine(int purchaseTax,Saleable saleable,short jnr)
        {
            var priceSummary = new PriceSummary(saleable,TaxMultiplier(purchaseTax));
             
            var unitPrice = saleable.Price / TaxMultiplier(purchaseTax);
             
            return new PricingLine(this, jnr, saleable.Name, saleable.Quantity, unitPrice, saleable.Unit, saleable.Discount, priceSummary.TotalWithoutVat, priceSummary.TotalWithVat);
        }


        protected void IssuedNowBy(Employee issuer)
        {
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
            IssuedOn = DateTime.Now;
        }

        public abstract string GetNumber();

        public  virtual DateTime? SentOn { get; protected set; }
        public  virtual DateTime? PrintedOn { get; }
        public  virtual string Email { get; protected set; }
        public  virtual string PartyName { get; protected set; }
        public  virtual string PartyAddress { get; protected set; }
        public  virtual string PartyCode { get; protected set; }
        public  virtual string VehicleLine1 { get; protected set; }
        public  virtual string VehicleLine2 { get; protected set; }
        public  virtual string VehicleLine3 { get; protected set; }
        public virtual string VehicleLine4 { get; protected set; }
        public  virtual DateTime IssuedOn { get; protected set; }
        public  virtual Employee Issuer { get; protected set; }
    }
}