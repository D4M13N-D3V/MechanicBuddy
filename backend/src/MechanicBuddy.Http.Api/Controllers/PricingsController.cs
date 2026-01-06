using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Domain;
using Carmasters.Http.Api.Model;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using static System.Collections.Specialized.BitVector32;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class PricingsController : ControllerBase
    {
        private readonly NHibernate.ISession repository;
        private readonly IPdfGenerator pdfGenerator;

        public PricingsController(NHibernate.ISession repository, IPdfGenerator pdfGenerator)
        {
            this.repository = repository;
            this.pdfGenerator = pdfGenerator;
        }

        [HttpGet("offers/{workId}")]
        public OkObjectResult GetAllOfferPricings(Guid workId) //todo better place?
        { 
            var issuances =  repository.Connection.Query<OfferIssuanceDto>(
                                  @"   select  
                                        o.id,
                                        est.number, 
                                        e.senton,
	                                    e.issuedon,
	                                    e.email as receiveremail,
	                                    i.firstname||' '||i.lastname as issuedby,
	                                    acceptedon,
	                                    (select firstname||' '||lastname from domain.employee where id = o.acceptorid)  as acceptedby 
	                                    from   domain.offer o
	                                    inner join domain.pricing e on e.id = o.estimateId
                                        inner join domain.estimate est on est.id = o.estimateid
	                                    inner join domain.employee i on i.id = e.issuerid where o.workid = @workId", new { workId = workId })
                                  .ToList();
             
            return Ok(issuances);
        }


        [HttpGet("invoice/{workId}/{type}")]
        public async Task<IActionResult> PrintInvoice(Guid workId,string type)
        {
            var invoiceId = repository.QueryOver<Work>()
                .Where(x => x.Id == workId)
                .Select(x => x.Invoice.Id)
                .SingleOrDefault<Guid>();

            var invoice = repository.Get<Invoice>(invoiceId);

            if(type == "pdf")
            {
                return await PdfResult(invoice);
            }
            return await HtmlResult(invoice);

        }

        [HttpGet("offer/{offerId}/{type}")]
        public async Task<IActionResult> PrintEstimatePdf(Guid offerId, string type)
        {
            var estimateId = repository.QueryOver<Offer>().Where(x => x.Id == offerId).Select(x => x.Estimate.Id).SingleOrDefault<Guid>();

            var estimate = repository.Get<Estimate>(estimateId);

            if (type == "pdf")
            {
                return await PdfResult(estimate);
            }
            return await HtmlResult(estimate);
        }
        private async Task<IActionResult> HtmlResult(Pricing pricing)
        {
            if (pricing == null) return NotFound();

            var body = await pdfGenerator.GetBodyGenerator().Generate(pricing);
           // var footer = await pdfGenerator.GetFooterGenerator().Generate(pricing);

            return new ContentResult()
            {
                Content = body,
                ContentType = "text/html",
            };
        }
        private async Task<IActionResult> PdfResult(Pricing pricing)
        {
            if (pricing == null) return NotFound();

             Response.Headers.Append("content-disposition", "inline;filename=" + pricing.GetFileName());
            var pdfBytes = await pdfGenerator.Generate(pricing);
            return File(pdfBytes, "application/pdf");
        }



    }
}
