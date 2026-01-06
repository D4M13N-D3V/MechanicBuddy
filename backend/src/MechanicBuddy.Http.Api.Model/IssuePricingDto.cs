using System;

namespace MechanicBuddy.Http.Api.Model
{
    public record IssuePricingDto(bool ShowVehicleOnPricing,bool SendClientEmail,string ClientEmail)
    {

    }
}
