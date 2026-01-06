using AutoMapper;
using Carmasters.Core.Application.Extensions;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Domain;
using Carmasters.Core.Repository.Postgres;
using Carmasters.Http.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : BaseController<RepairJobDto, RepairJob>
    {
        public JobsController(IRepository repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
