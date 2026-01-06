using System;
using System.Linq;
using AutoMapper;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Domain;
using Carmasters.Core.Repository.Postgres;
using Carmasters.Http.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers
{
    [TenantRateLimit]
    [Route("api/[controller]")]
    [ApiController]
    public class SparePartsController : BaseController<SparePartDto, SparePart>
    {

        public SparePartsController(IRepository repository, IMapper mapper) : base(repository, mapper)
        {

        }



        [HttpGet("page")]
        public PagedResult<SparePartDto> GetPage(string searchText, string orderby, int limit, int offset, bool desc)
        {
            return
               repository
                 .PageQuery<SparePartDto>(orderby, limit, offset, desc)
                 .FilterBy(searchText)
                 .SearchFields("concat_ws(' ',code,sparepart.name,s.name)")
                 .SelectSql(@"select sparepart.*,s.name as storagename
                                    from domain.sparepart left join domain.storage s on s.id = storageid  ")
                 .ToResult();
        }

        [Authorize(Policy = "ServerSidePolicy")]
        [HttpPut("/api/spareparts/{id}/umprice")]
        public void FreshenUmPrice(Guid id, UmPriceDto model)
        {
            var sparePart = repository.Get<SparePart>(id);

            sparePart.PriceFromUm(model.Price, model.Name, model.Address);

            repository.Update(sparePart);

        }
        //TODO move custom method without out that are not api methods
        protected override SparePart CreateFrom(SparePartDto model)
        {

            var storage = model.StorageId is not null ? repository.Get<Storage>(model.StorageId.GetValueOrDefault()) : null;
            var spare = new SparePart(model.Code, model.Name, model.Price, model.Quantity, model.Discount, model.Description, DateTime.Now, storage);
            return spare;
        }

        protected override void Edit(SparePart entity, SparePartDto model)
        {
            entity.Edit(model.Code, model.Name, model.Price, model.Quantity, model.Discount, model.Description);
            if (model.StorageId.HasValue)
            {
                entity.StoredAt(repository.Get<Storage>(model.StorageId.Value));
            }
        } 
    }



}
