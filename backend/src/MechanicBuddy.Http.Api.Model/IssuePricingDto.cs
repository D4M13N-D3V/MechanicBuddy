using System;

namespace Carmasters.Http.Api.Model
{
    public record IssuePricingDto(bool ShowVehicleOnPricing,bool SendClientEmail,string ClientEmail)
    {

    }
}
