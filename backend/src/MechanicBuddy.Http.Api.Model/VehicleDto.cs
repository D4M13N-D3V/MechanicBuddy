using System;

namespace Carmasters.Http.Api.Models
{
    public record VehiclePageDto(string Producer, string Model, string RegNr, string Vin, string Body, string Engine, DateTime? ProductionDate, string Region, string Series, string Transmission, Guid Id, string OwnerName, Guid? OwnerId)
    {
        public VehiclePageDto() : this(default, default, default, default, default,default,default,default,default,default,default,default,default) { }
    }

    public class ClientVehicleDto
    {
        public Guid Id { get; set; } 
        public Guid? OwnerId { get; set; }
        public string Producer { get; set; }
        public string Model { get; set; }
        public string RegNr { get; set; }
        public string Vin { get; set; }
      
    }
    public class VehicleDto
    {
        public string Producer { get; set; }
        public string Model { get; set; }
        public string RegNr { get; set; }
        public string Vin { get; set; }
        public int Odo { get; set; }

        public string Body { get; set; }
        public string DrivingSide { get; set; }
        public string Engine { get; set; }
        public DateTime? ProductionDate { get; set; }
        public string Region { get; set; }
        public string Series { get; set; }
        public string Transmission { get; set; }
        public Guid Id { get; set; }

        public Guid? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string Description { get; set; }
        public DateTime IntroducedAt { get; set; } 
    } 

}