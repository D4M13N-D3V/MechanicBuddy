using AutoMapper;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Domain;
using Carmasters.Http.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers.Clients
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class LegalClientsController : BaseController<LegalClientDto, LegalClient>
    {
        public LegalClientsController(IRepository repository, IMapper mapper) : base(repository, mapper)
        {
        }

        protected override LegalClient CreateFrom(LegalClientDto model)
        {

            var client = new LegalClient(
                model.IntroducedAt,
                model.Name,
                model.RegNr,
               ClientsController.CreateAddress(model.Address),
                model.Phone,
                model.IsAsshole);

            client.UsesEmail(model.EmailAddresses, model.CurrentEmail);
            return client;
        }


        protected override void Edit(LegalClient entity, LegalClientDto model)
        {
            entity.Edit(model.Name, model.RegNr,
                ClientsController.CreateAddress(model.Address), model.Phone, model.IsAsshole, model.Description);
            entity.UsesEmail(model.EmailAddresses, model.CurrentEmail);
        }
    }
}
