using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Carmasters.Core.Domain
{ 
    public  class Work : GuidIdentityEntity
    {
        Client client;
        IList<Offer> offers = new List<Offer>();
        IList<RepairJob> jobs = new List<RepairJob>();
        private IList<Assignment> assignements = new List<Assignment>(); 
        public virtual IReadOnlyCollection<Offer> Offers { get => this.offers.ToList().AsReadOnly(); }
        public virtual IReadOnlyCollection<RepairJob> Jobs { get => this.jobs.ToList().AsReadOnly(); }
        public virtual IReadOnlyCollection<Employee> Mechanics => assignements.Select(x => x.Mechanic).ToList().AsReadOnly();
        protected Work() { }
        public  Work(int number,DateTime startedOn, Employee starter, Client client = null, Vehicle vehicle = null,Invoice invoice = null,string notes = null, int? odo = null, Guid? id = null)
        {
            Id = id.GetValueOrDefault();
            StartedOn = startedOn;
            this.ChangedOn = DateTime.Now;
            this.Starter = starter ?? throw new ArgumentNullException(nameof(starter));
            this.client = client;
            this.Vehicle = vehicle;
            this.Notes = notes;
            this.Odo = odo;
            this.Invoice = invoice;
            this.Number = number;
        }
      
        public virtual async Task<Offer> Issue(Offer offer, IPricingSender sender, int purchaseTax, Employee issuer, bool showVehicleOnPricing, bool sendClientEmail, string clientEmail)
        {
            if (sendClientEmail && string.IsNullOrWhiteSpace(clientEmail))
            {
                throw new UserException("Cannot send an estimate, client email not provided.");
            }
            
            if (offer.Estimate != null)   //reissuing has to create a new copy, most likely current issued version is already out there , cant have different versioins of offer with same number
            {
                offer = offer.MakeCopy(this, issuer);
                this.offers.Add(offer); 
            }

            offer.Issue(purchaseTax, issuer, showVehicleOnPricing);

            if (sendClientEmail)
            {
                await offer.SendEstimate(sender, clientEmail);
            }
            return offer;
        }
        public virtual WorkStatus UserStatus { get; protected set; }
        public virtual void ChangeState(WorkStatus status)
        {
            if (this.Invoice is not null && status == WorkStatus.Closed) throw new UserException("Cannot close, invoice issued.");

            this.UserStatus = status;
        }
         

        public virtual Work CreateCopy( int newNumber,Employee starter)
        {
            var work = new Work(newNumber,DateTime.Now,starter,Client,Vehicle,null,notes:Notes,odo:Odo);
            foreach (var job in jobs)
            {
                work.jobs.Add(job.MakeCopy(work, starter));
            }
            foreach (var offer in offers)
            {
                work.offers.Add(offer.MakeCopy(work, starter));
            }
            return work;
        }
          

        public virtual void Complete(Employee completer)
        {
            this.Completer = completer;
            this.CompletedOn = DateTime.Now;
        }

        public virtual void Assign(Employee mechanic) 
        {
            this.Assign(new[] { mechanic });
        }
        public virtual void Assign(Employee[] mechanics)
        { 
            
            foreach (var mechanic in mechanics)
            {
                if (!this.assignements.Any(x => x.Mechanic == mechanic)) 
                {
                    this.assignements.Add(new Assignment(this, mechanic));
                }
            }
            var toRemove = assignements.Where(assignment => !mechanics.Any(x => x == assignment.Mechanic)).ToList();
            foreach (var assignment in toRemove)
            {
                this.assignements.Remove(assignment);
            } 
        }
         
        public virtual void GenerateInvoice(ISequnceNumberProviderFactory numberProvider, int purchaseTax, PaymentType paymentType, short dueDays, Employee issuer)
        {
            var number = numberProvider.
                GetNumberProvider<Invoice>();

            this.Invoice = Invoice.CreateFor(this,  number,purchaseTax, paymentType, dueDays, issuer); 
        }
  
        public virtual void WithNotes(string description)
        {
            this.Notes = description;
        }
         
        public virtual void WithoutVehicle()
        {
            this.Vehicle = null;
        }

       
        public  static Work Start(ISequnceNumberProviderFactory numberProvider, Employee starter,Client client = null,  Vehicle vehicle = null, string notes= null, int? odo = null)
        {
            var number = numberProvider.
                GetNumberProvider<Work>().Next();

            return new Work(number,DateTime.Now,starter,client,vehicle,null,notes,odo); 
        }

        public virtual int Number { get; protected set; }
        public virtual int? Odo { get; protected set; }
        public virtual Employee Starter { get; protected set; }
        public virtual DateTime? CompletedOn { get; protected set; }
        public virtual Employee Completer { get; protected set; } 
        public virtual string Notes { get; protected set; }
        public virtual DateTime StartedOn { get; protected set; }

        public virtual DateTime ChangedOn { get; protected set; } //todo protected and user id too?

        public  virtual Client Client { get { return client; } } 
        public  virtual Vehicle Vehicle { get; protected set; }
        public  virtual Invoice Invoice { get; protected set; } 
        public  virtual void DoneOn(Vehicle vehicle)
        {
            this.Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
        }
        public  virtual void IsForPrivateClient()
        {
            this.client = null;
        }
        public  virtual void IsFor(Client client)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            this.client = client;
        }
         
        public  virtual Offer CreateOffer(Employee starter,string notes = null)
        {
            var offer = Offer.Create(this, starter,notes);
            this.offers.Add(offer);
            return offer;
        }
        public virtual RepairJob StartRepairJob(Employee starter, string notes = null)
        {
            var job = RepairJob.Create(this, starter, notes);
            this.jobs.Add(job);
            return job;
        }
        
        public virtual void Remove(Offer offer)
        {
            this.offers.Remove(offer);
        }
        public virtual void Remove(RepairJob offer)
        {
            this.jobs.Remove(offer);
        }

        public virtual void DeleteInvoice(ISequnceNumberProviderFactory numberProviderFactory)
        {
            var newNumber = numberProviderFactory.GetNumberProvider<Invoice>().Next();
            if(newNumber!= (this.Invoice.Number+1)) // only the last one can be deleted, invoice order thing, numbers must go in sequence.
            {
                throw new UserException("Cannot delete an invoice, only last invoice can be deleted.");
            }
            this.Invoice = null;
        }

        public virtual void Changed()
        {
            ChangedOn = DateTime.Now;
        }
    }
}