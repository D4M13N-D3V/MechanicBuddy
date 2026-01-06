using System;

namespace Carmasters.Core.Domain
{
    public class ServicePerformed : Saleable
    {
        private readonly RepairJob job;
        protected ServicePerformed() : base() { }
        internal  ServicePerformed(RepairJob job, string name,decimal quantity,string unit,decimal price,string notes = null, short? discount = null, Guid? id = null) : base(name, quantity, unit, price, discount, id)
        {
            this.job = job;
            Notes = notes;
        }
          
        public  virtual string Notes { get; }
        public  virtual Employee Mechanic { get; protected set; }

        public  virtual void PerformedBy(Employee mechanicl)
        {
            this.Mechanic = mechanicl;
        }
    }
}