using AutoMapper;
using Carmasters.Core.Application.Extensions;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Domain;
using Carmasters.Http.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Linq;
using System.Reflection;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers.Clients
{
    [TenantRateLimit]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : BaseController<ClientPageDto, Client>
    {
        public ClientsController(IRepository repository, IMapper mapper) : base(repository, mapper)
        {
        }


        [HttpGet("page")]
        public PagedResult<ClientPageDto> GetPage(string searchText, string orderby, int limit, int offset, bool desc)
        {

            return
                repository
                  .PageQuery<ClientPageDto>(orderby, limit, offset, desc)
                  .FilterBy(searchText)
                  .SearchFields("concat_ws(' ',firstname,lastname,l.name,ce.address,c.phone,c.address)")
                  .SelectSql(@"SELECT 
                                    c.id, 
                                    concat_ws(' ',c.country,c.region,c.city,c.address,c.postalcode)  as address,
                                    ce.address AS email,  
                                    c.introducedat,
                                    (l.id IS NOT NULL) AS iscompany,
                                    concat_ws(' ',firstname,lastname,l.name)  as name, 
                                    c.phone
                                FROM domain.client AS c
                                    left join domain.clientemail ce on ce.clientid = c.id and ce.isactive
                                    LEFT JOIN domain.legalclient AS l ON c.id = l.id
                                    LEFT JOIN domain.privateclient AS p ON c.id = p.id")
                  .ToResult();
        }

         
        [Authorize(Policy = "ServerSidePolicy")]
        public override ActionResult Get(Guid id)
        {
            var client = repository.Get<Client>(id, false);
            if (client is PrivateClient) return LocalRedirect("/api/privateclients/" + id);
            if (client is LegalClient) return LocalRedirect("/api/legalclients/" + id);
            return base.NotFound();
        }

        public static AddressComponent CreateAddress(AddressDto addressDto)
        {
            return new AddressComponent(
               addressDto?.Street,
               addressDto?.Country,
               addressDto?.Region,
               addressDto?.City,
               addressDto?.PostalCode
            );
        }
    }
}
