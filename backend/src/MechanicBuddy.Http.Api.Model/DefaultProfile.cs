using AutoMapper;
using Carmasters.Core.Domain;
using Carmasters.Http.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carmasters.Http.Api.Model
{
    public class DefaultProfile : AutoMapper.Profile
    {
        public DefaultProfile()
        {

            this.CreateMap<Carmasters.Core.Domain.Employee, EmployeeDto>();
           
           
            this.CreateMap<Carmasters.Core.Domain.Storage, StorageDto>();

            this.CreateMap<Carmasters.Core.Domain.SparePart, SparePartDto>()
                .ForMember(x => x.StorageId, m => m.MapFrom(x => x.Storage == null ? (Guid?)null : x.Storage.Id));

            this.CreateMap<ClientEmail, string>().ConvertUsing(c => c.Address);
           // this.CreateMap<short, PaymentType>().ConvertUsing(c => (PaymentType)(int)c);
            this.CreateMap<Carmasters.Core.Domain.PrivateClient, PrivateClientDto>();
            this.CreateMap<Carmasters.Core.Domain.LegalClient, LegalClientDto>();

            this.CreateMap<AddressComponent, AddressDto>()
                .ForMember(x => x.Street, m => m.MapFrom(x => x.Street));
                ;
              
             
        } 

    }
     
}
