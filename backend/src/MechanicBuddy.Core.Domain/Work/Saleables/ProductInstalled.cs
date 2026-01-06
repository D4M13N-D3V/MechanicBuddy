using System;

namespace Carmasters.Core.Domain
{
    public class ProductInstalled : Product
    {
        public virtual RepairJob Job { get; }
        protected ProductInstalled() : base() { }
        public ProductInstalled(RepairJob job,
                                 short jnr,
                                 string code, 
                                string name,
                                decimal? quantity,
                                string unit,
                                decimal price, 
                                short? discount = null,
                                Guid? id = null) : base(jnr,code,name, quantity, unit, price, discount, id)
        {
            this.Job = job; 
            Status = ProductInstallStatus.Paigaldatud;
            Notes = null;
           
        }

       
        public  virtual ProductInstallStatus Status { get; protected set; }
        public  virtual string Notes { get; protected set; }
        public  virtual ServicePerformed Service { get; protected set; }

        public  virtual void WithIn(ServicePerformed service)
        {
            this.Service = service;
        }

        protected internal virtual ProductInstalled MakeCopy(RepairJob job)
        {
            return new ProductInstalled(job, Jnr, Code, Name, Quantity, Unit, Price, Discount);
        }
    }
}