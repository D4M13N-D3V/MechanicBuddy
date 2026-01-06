using System;
using System.Collections.Generic;

namespace Carmasters.Core.Domain
{
    public class VehicleRegistration  
    {  
        protected VehicleRegistration() { }
        protected  VehicleRegistration(Vehicle vehicle, DateTime dateTimeFrom, DateTime? dateTimeTo)
        {
            this.vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
            DateTimeFrom = dateTimeFrom;
            DateTimeTo = dateTimeTo;
        }

        public  static VehicleRegistration New(Vehicle vehicle,Client owner) 
        {
            if (owner is null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            return new VehicleRegistration(vehicle,DateTime.Now, null) { Owner = owner };
        }
        private readonly Vehicle vehicle; 
        public  virtual Client Owner { get; protected set; }
        public  virtual DateTime DateTimeFrom { get; }
        public  virtual DateTime? DateTimeTo { get; protected set; }
        public  virtual bool IsEffective => DateTimeTo == null;

        public  virtual void End() 
        {
            this.DateTimeTo = DateTime.Now;
        }

        public override bool Equals(object obj)
        {
            return obj is VehicleRegistration registration &&
                   Owner == registration.Owner &&
                   vehicle == registration.vehicle &&
                   DateTimeFrom == registration.DateTimeFrom;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Owner, vehicle, DateTimeFrom);
        }
    }
}