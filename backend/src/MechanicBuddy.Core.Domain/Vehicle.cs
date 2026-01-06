using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace Carmasters.Core.Domain
{
    public class Vehicle: GuidIdentityEntity
    {
        IList<VehicleRegistration> registrations = new List<VehicleRegistration>();
        protected Vehicle() { }
        public    Vehicle(
            string regNr,
            DateTime introducedAt,
            string producer = null,
            string model = null, 
            string vin = null,
            int? odo = null,
            string body = null,
            string drivingSide = null,
            string engine = null,
            DateTime? productionDate = null,
            string region = null,
            string series = null,
            string transmission = null,
            string description = null,
            Guid? id = null)
        {  
            IntroducedAt = introducedAt; 
            SetValues(regNr, producer, model, vin, odo, body, drivingSide, engine, productionDate, region, series, transmission,description);
            this.Id = id.GetValueOrDefault();
        }


        private void SetValues(string regNr,
                               string producer,
                               string model,
                               string vin,
                               int? odo,
                               string body,
                               string drivingSide,
                               string engine,
                               DateTime? productionDate,
                               string region,
                               string series,
                               string transmission,
                               string description)
        {
            if (string.IsNullOrWhiteSpace(regNr)&& string.IsNullOrWhiteSpace(vin))
            {
                throw new UserException("Vehicle registration number or VIN code is required.");
            }
            Description = description;
            Producer = producer;
            Model = model;
            RegNr = regNr;
            Vin = vin;
            Odo = odo;
            Body = body;
            DrivingSide = drivingSide;
            Engine = engine;
            ProductionDate = productionDate;
            Region = region;
            Series = series;
            Transmission = transmission;
        }
         
        public  virtual string Producer { get; protected set; }
        public  virtual string Model { get; protected set; }
        public  virtual string RegNr { get; protected set; }
        public  virtual string Vin { get; protected set; }
        public  virtual int? Odo { get; protected set; }
        public  virtual string Body { get; protected set; }
        public  virtual string DrivingSide { get; protected set; }
        public  virtual string Engine { get; protected set; }
        public  virtual DateTime? ProductionDate { get; protected set; }
        public  virtual string Region { get; protected set; }
        public  virtual string Series { get; protected set; }
        public  virtual string Transmission { get; protected set; }

        public  virtual Client Owner { get 
            { 
                return registrations.SingleOrDefault(x=>x.IsEffective)?.Owner; 
            } 
        }

        public  virtual IReadOnlyCollection<VehicleRegistration> Registrations { get => registrations.ToList().AsReadOnly(); }

        public  virtual void Edit(string regNr,
            string producer = null,
            string model = null,
            string vin = null,
            int odo = 0,
            string body = null,
            string drivingSide = null,
            string engine = null,
            DateTime? productionDate = null,
            string region = null,
            string series = null,
            string transmission = null,
            string description = null)
        {
            SetValues(regNr, producer, model, vin, odo, body, drivingSide, engine, productionDate, region, series, transmission,description);
        }


        public  virtual void RegisterTo(Client owner)
        {
            if (this.Owner!=null && this.Owner.Equals(owner)) throw new Exception("already registered to this owner");
            //end previous reg 
            this.EndRegistration();

            this.registrations.Add(VehicleRegistration.New(this,owner));
        }

        public virtual void EndRegistration() 
        {
            this.registrations
               .SingleOrDefault(x => x.IsEffective)?.End();
        }


        public  virtual DateTime IntroducedAt { get; }
        public  virtual string Description { get; protected set; }
    }

}