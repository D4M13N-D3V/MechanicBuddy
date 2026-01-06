 

using System;

public class PricingDto
{
    public DateTime SentOn { get; set; }
    public DateTime PrintedOn { get; set; }
    public string Email { get; set; }
    public string PartyName { get; set; }
    public string PartyAddress { get; set; }
    public string PartyCode { get; set; }
    public string VehicleLine1 { get; set; }
    public string VehicleLine2 { get; set; }
    public string VehicleLine3 { get; set; }
    public DateTime IssuedOn { get; set; }
    public Guid IssuerId { get; set; }
    public Guid Id { get; set; }
}
