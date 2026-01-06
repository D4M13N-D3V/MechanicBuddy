using AutoMapper;
using Carmasters.Core.Domain;
using Carmasters.Http.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Carmasters.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using System;
using NHibernate;
using System.Linq;
using NHibernate.Mapping;
using System.Collections.Generic;
using Dapper;
using Carmasters.Core.Application.RateLimiting;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers
{
    /// <summary>
    /// todo refract not http methods out
    /// </summary>
    [TenantRateLimit]
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : BaseController<VehicleDto, Vehicle>
    {
        private readonly ISession session;

        public VehiclesController(IRepository repository,ISession session, IMapper mapper) : base(repository, mapper)
        {
            this.session = session;
        }

        [HttpGet("client/{clientId}")]
        public ClientVehicleDto[] ClientVehicles(Guid clientId)
        {
            var vehicles = repository.GetConnection()
               .Query<ClientVehicleDto>(@" select v.producer,v.model,v.regnr,v.vin,v.id,r.ownerid from domain.vehicleregistration r
										    inner join domain.vehicle v on v.id = r.vehicleid
											  where r.ownerid = @ownerid  
												  and r.datetimeto is null", new { ownerid = clientId })
               .ToArray();


            return vehicles;
        }
       
        protected override VehicleDto Map(Vehicle entity)
        {
            return new VehicleDto
            {
                Body = entity.Body,
                Description = entity.Description,
                DrivingSide = entity.DrivingSide,
                Engine = entity.Engine,
                Id = entity.Id,
                IntroducedAt = entity.IntroducedAt,
                Model = entity.Model,
                Odo = entity.Odo.GetValueOrDefault(),
                OwnerId = entity.Owner?.Id,
                OwnerName = entity.Owner?.Name,
                Producer = entity.Producer,
                ProductionDate = entity.ProductionDate,
                Region = entity.Region,
                RegNr = entity.RegNr,
                Series = entity.Series,
                Transmission = entity.Transmission,
                Vin = entity.Vin
            };
        }


        protected override Vehicle CreateFrom(VehicleDto model)
        {
            var vehicle = new Vehicle(model.RegNr,
                        model.IntroducedAt,
                        model.Producer,
                        model.Model,
                        model.Vin,
                        model.Odo,
                        model.Body,
                        model.DrivingSide,
                        model.Engine,
                        model.ProductionDate,
                        model.Region,
                        model.Series,
                        model.Transmission,
                        model.Description);
            if (model.OwnerId != null)
            {
                vehicle.RegisterTo(repository.Get<Client>(model.OwnerId.GetValueOrDefault()));
            }
            return vehicle;
        }

        protected override void Edit(Vehicle entity, VehicleDto model)
        {
            entity.Edit(model.RegNr,
                        model.Producer,
                        model.Model,
                        model.Vin,
                        model.Odo,
                        model.Body,
                        model.DrivingSide,
                        model.Engine,
                        model.ProductionDate,
                        model.Region,
                        model.Series,
                        model.Transmission,
                        model.Description);
            if (model.OwnerId != null)
            {
                if (entity.Owner?.Id != model.OwnerId.GetValueOrDefault()) 
                {
                    entity.RegisterTo(repository.Get<Client>(model.OwnerId.GetValueOrDefault()));
                }
                
            }
            else entity.EndRegistration();
        }

        [HttpGet("page")]
        public PagedResult<VehiclePageDto> GetPage(string searchText, string orderby, int limit, int offset, bool desc)
        {
            return
                repository
                  .PageQuery<VehiclePageDto>(orderby, limit, offset, desc)
                  .FilterBy(searchText)
                  .SearchFields("concat_ws(' ',v.regnr,vin,firstname,lastname,l.name,producer,model)") //,body,drivingside,engine,TO_CHAR(productiondate,'MM-YYYY'),region,series,transmission
                  .SelectSql(@"SELECT 
                        v.id, 
                        v.regnr,vin, producer,model,body,drivingside,engine,TO_CHAR(productiondate,'MM-YYYY') as productiondate,region,series,transmission,
                        concat_ws(' ',firstname,lastname,l.name)  as ownername,
                        v0.ownerid as ownerid
                        FROM domain.vehicle AS v
                               left join domain.vehicleregistration v0 on v.id = v0.vehicleid AND v0.datetimeto IS NULL
	                           left join domain.legalclient l on l.id = v0.ownerid
                               left join domain.privateclient p on p.id = v0.ownerid")
                  .ToResult();

        }
    }
}
