using System.Linq;
using Carmasters.Core.Application;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHibernate;

namespace Carmasters.Http.Api.Controllers
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class QueryController : ControllerBase
    {
        private readonly ISession session;

        public QueryController(ISession session)
        {
            this.session = session;
        }

        [HttpGet("{searchText}")]
        public dynamic Get(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return new dynamic[0];

            var sql = @"select * from ( 
                        select id,'Client' as resourcename, concat_ws(' ',firstname,lastname) as name,'klient' as controller from domain.privateclient
                        union all
                        select id,'Client' as resourcename,concat_ws(' ',name,(case when regnr is null or regnr='' then null else '('||regnr||')' end)) as name,'klient' as controller from domain.legalclient
                        union all
                        select id, 'Vehicle' as resourcename,concat_ws(' ',regnr,(case when vin is null or vin='' then null else '('||vin||')' end)) as name,'soiduk' as controller  from domain.vehicle  
                        union all
                        select id,'Work nr. ' as resourcename, ''||id as name, 'too' as controller from domain.work
                        union all
                        select work.id, 'Invoice nr. ' as resourcename, ''||invoice.number as name,'too' as controller from domain.invoice inner join domain.work on work.invoiceid = invoice.id
                        union all
                        select work.id, 'Estimate nr. ' as resourcename, ''||offer.id as name,'too' as controller from domain.offer inner join domain.work on work.id = offer.workid
                        union all
                        select id, 'Sparepart' as resourcename , code as name, 'varuosa' as controller from domain.sparepart
                        union all
                        select id, 'Employer' as resourcename,concat_ws(' ',firstname,lastname) as name,'tootaja' as controller from domain.employee 
                        ) results   
	                        where to_tsvector(name) @@ to_tsquery(@arg)
                        limit 10";
            var arg = new WildcardTokens(searchText).ToString();

            var results = session.Connection.Query(sql, new { arg }).ToList();

            return results;
        }
    }
}
