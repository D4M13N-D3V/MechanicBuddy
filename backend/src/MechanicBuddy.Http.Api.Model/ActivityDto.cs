using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmasters.Http.Api.Model
{
    public record ActivityDto(Guid Id, string Number,  DateTime StartedOn, string StartedBy, string Name,string Notes,bool IsVehicleLinesOnPricing,bool IsEmpty)  
    { 
        public ActivityDto():this( default,default,default,default,default,default,default,default)
        {
        }
    }
}
