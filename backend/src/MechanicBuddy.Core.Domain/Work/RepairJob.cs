using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Carmasters.Core.Domain
{
    public class RepairJob : GuidIdentityEntity
    {
        protected readonly Work work;
        private IList<ProductInstalled> products = new List<ProductInstalled>();
        private IList<ServicePerformed> services = new List<ServicePerformed>();
        protected RepairJob() { }
        public RepairJob( Work work,   DateTime startedOn, Employee starter = null,   string notes = null,  Guid? id = null)
        {
            this.Id = id.GetValueOrDefault();
            this.work = work;  
            this.Notes = notes;
            this.StartedOn = startedOn;
            this.Starter = starter;
            OrderNr = work.Jobs.Any() ? (short)(work.Jobs.Max(x=>x.OrderNr)+1) : (short)0;
        }

        public virtual Work Work => work;
        public virtual string Notes { get; protected set; } 
        public virtual Int16 OrderNr { get; protected set; }
         
        public virtual DateTime StartedOn { get; protected set; }
        public virtual Employee Starter { get; protected set; }
        public virtual IReadOnlyCollection<ProductInstalled> Products { get => this.products.ToList().AsReadOnly(); }
        public virtual IReadOnlyCollection<ServicePerformed> Services { get => this.services.ToList().AsReadOnly(); }
        
        public static RepairJob Create(  Work work,Employee starter, string notes)
        {
            return new RepairJob( work, DateTime.Now, starter,notes: notes);
        }
          
        public virtual void With(string notes)
        {
            this.Notes = notes;
        }

        public virtual void CopySaleablesFrom(Offer offer, Employee acceptor)
        { 
            var lastJnr = this.products.Any() ? products.Max(x => x.Jnr) : 0;
            foreach (var newProduct in offer.Products.Select((p, i) => new ProductInstalled(this, Convert.ToInt16(lastJnr + i + 1), p.Code, p.Name, p.Quantity, p.Unit, p.Price, p.Discount)))
            {
                products.Add(newProduct);
            }
        }
        public virtual ServicePerformed AddService(string name, decimal quantity, string unit, decimal price, string notes = null, short? discount = null)
        {
            var service = new ServicePerformed(this, name, quantity, unit, price, notes, discount);
            services.Add(service);
            return service;
        }
        public virtual void With(ProductInstalled[] updateProducts)
        {
            Product.Synchronize(updateProducts, this.products);
        }


        protected internal virtual RepairJob MakeCopy(Work work,Employee starter)
        {
            var job = new RepairJob(work, DateTime.Now, starter, notes: Notes);

            foreach (var product in products)
            {
                job.products.Add(product.MakeCopy(job));
            }

            return job;

        }
    }
}