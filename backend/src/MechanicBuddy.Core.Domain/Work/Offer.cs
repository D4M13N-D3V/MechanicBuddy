using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Carmasters.Core.Domain
{
    public class Offer : GuidIdentityEntity
    {
        protected readonly Work work;
        private IList<ProductOffered> products = new List<ProductOffered>();
        private IList<ServiceOffered> services = new List<ServiceOffered>();
        protected Offer() { }
        public  Offer(Work work,   Estimate estimate, DateTime startedOn, Employee starter,Employee acceptor = null,DateTime? acceptedOn = null,string notes = null,bool isVehicleLinesOnEstimate= false,Guid? id = null)
        {
            this.Id = id.GetValueOrDefault();
            this.work = work; 
            Estimate = estimate;
            AcceptedOn = acceptedOn;
            this.Notes = notes;
            this.StartedOn = startedOn;
            this.Starter = starter;
            this.Acceptor = acceptor;
            this.OrderNr = work.Offers.Any() ? (short)(work.Offers.Max(x=>x.OrderNr) + 1) : (short)0;
        }

        public virtual Work Work => work;
        public virtual Int16 OrderNr { get; protected set; }
         
        public virtual string Notes { get; protected set; }
        public virtual bool IsVehicleLinesOnEstimate { get; protected set; }
        public virtual Employee Starter { get; protected set; }
        public virtual DateTime StartedOn { get; protected set; } 
        public  virtual IReadOnlyCollection<ProductOffered> Products { get => this.products.ToList().AsReadOnly(); }
        public  virtual IReadOnlyCollection<ServiceOffered> Services { get => this.services.ToList().AsReadOnly(); }

        public  virtual Estimate Estimate { get; protected set; }
        public  virtual DateTime? AcceptedOn { get; protected set; }

        public  virtual Employee Acceptor { get; protected set; } 

        public  static Offer Create(Work work,Employee starter,  string notes)
        {
            return new Offer(work,null,DateTime.Now, starter,notes: notes);
        }
          
        public virtual RepairJob Accepted(short? targetJobNumber,string notes, Employee acceptor)
        {
            this.Acceptor = acceptor;
            this.AcceptedOn = DateTime.Now;
            var targetJob =   work.Jobs.SingleOrDefault(x => x.OrderNr == targetJobNumber);
            if (targetJob == null) //create new job
            {
                targetJob = this.work.StartRepairJob(acceptor, notes);
            }
            targetJob.CopySaleablesFrom(this, acceptor);
            return targetJob;
        }


        protected internal virtual void Issue(int purchaseTax, Employee issuer, bool showVehicleOnPricing)
        {
            this.DisplayVehicleOnEstimate(showVehicleOnPricing);

            var number = $"{this.Work.Number}-{this.OrderNr}";
            With(new Estimate(number)
                .CreateFor(purchaseTax, this, issuer));
        }

        protected internal virtual async Task SendEstimate(IPricingSender sender, string clientEmail)
        {
            await Estimate.Send(sender, clientEmail);
        }
         
        public virtual void With(Estimate estimate)
        {
            this.Estimate = estimate;
        }

        public virtual Estimate CancelEstimate() 
        {
            var estimate = this.Estimate;

            this.Estimate = null; 
            return estimate;
        }

        public virtual void With(string notes) 
        {
            this.Notes = notes;
        }
        public virtual void With(ProductOffered[] updateProducts)
        {
            Product.Synchronize(updateProducts, this.products);
        } 
         
        public virtual ServiceOffered AddService(string name, decimal quantity, string unit, decimal price,short? discount = null)
        {
            var service = new ServiceOffered(this, name, quantity, unit, price,discount);
            services.Add(service);
            return service;
        }

        public virtual ProductOffered AddProduct(short jnr,string productCode, string name, decimal? quantity, string unit, decimal price, short? discount = null)
        {
            var productOffered = new ProductOffered(this,jnr, productCode, name, quantity, unit, price, discount);
            products.Add(productOffered);
            return productOffered;

        }

        public virtual void DisplayVehicleOnEstimate(bool isVehicleLinesOnEstimate)
        {
            this.IsVehicleLinesOnEstimate = isVehicleLinesOnEstimate;
        }

        protected internal virtual Offer MakeCopy(Work work,Employee starter)
        {
            var offer = new Offer(work, null, DateTime.Now, starter, null,null, notes: Notes, IsVehicleLinesOnEstimate);

            foreach (var product in this.products)
            {
                offer.products.Add(product.MakeCopy(offer));
            }

            return offer;

        }

      
    }

    //public class EstimateNumber
    //{
    //    private string number;

    //    private EstimateNumber(string number)
    //    {
    //        this.number = number;
    //    }
       
    //    //public static EstimateNumber Create(Work work, Offer offer)
    //    //{

    //    //    return new EstimateNumber( offer.OrderNr == 0?work.Number.ToString() :$"{work.Number}-{offer.OrderNr}");
    //    //}

    //    public override string ToString()
    //    {
    //        return number;
    //    }
    //}
}