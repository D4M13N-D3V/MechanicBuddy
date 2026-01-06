using System;
using System.Linq;
using AutoMapper;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Repository.Postgres;
using Carmasters.Http.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class StoragesController : BaseController<StorageDto, Core.Domain.Storage>
    {

        public StoragesController(Core.Domain.IRepository repository, IMapper mapper) : base(repository, mapper)
        {
        }
        [HttpGet()]
        public virtual ActionResult Get()
        {
            var locations = repository.GetConnection()
               .Query(@"select id,name from domain.storage").Select(x =>
                new
                {
                    Id = x.id,
                    Name = x.name
                }).ToArray();

            return new JsonResult(locations);
        }

        [HttpGet("page")]
        public PagedResult<StorageDto> GetPage(string searchText, string orderby, int limit, int offset, bool desc)
        {
            return
              repository
                .PageQuery<StorageDto>(orderby, limit, offset, desc)
                .FilterBy(searchText)
                .SearchFields("concat_ws(' ',name,address,description)")
                .SelectSql(@"select * from domain.storage")
                .ToResult();
        }

        protected override Core.Domain.Storage CreateFrom(StorageDto model)
        {
            return new Core.Domain.Storage(  model.Name, model.Address, model.Description, model.IntroducedAt);
        }

        protected override void Edit(Core.Domain.Storage entity, StorageDto model)
        {
            entity.ChangeName(model.Name);
            entity.ChangeAddress(model.Address);
            entity.ChangeDescription(model.Description);
        }

    }
}
