using AutoMapper;
using Carmasters.Core.Application;
using Carmasters.Core.Application.Configuration;
using Carmasters.Core.Application.Extensions;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Domain;
using Carmasters.Http.Api.Model;
using Carmasters.Http.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NHibernate; 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq; 
using System.Threading.Tasks; 
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    public class WorkController : ControllerBase
    { 
        private readonly IRepository repository;
        protected readonly IMapper mapper;
        private readonly ISequnceNumberProviderFactory numberProviderFactory;
        private readonly ISession session;
        private readonly IConfiguration configuration;
        private readonly IPricingSender pricingSender;
        private readonly ITenantConfigService tenantConfigService;
        static CultureInfo cultureUS = new CultureInfo("en-US");
        public WorkController(IRepository repository, IMapper mapper, ISequnceNumberProviderFactory numberProviderFactory, ISession session, IConfiguration configuration,IPricingSender pricingSender, ITenantConfigService tenantConfigService)
        { 
            this.repository = repository;
            this.mapper = mapper;
            this.numberProviderFactory = numberProviderFactory;
            this.session = session;
            this.configuration = configuration;
            this.pricingSender = pricingSender;
            this.tenantConfigService = tenantConfigService;
        }

         
        [HttpGet("{id}")]
        public dynamic Get(Guid id)
        { 
            var work = session.Get<Work>(id);

            var status = work.Invoice is not null ? "completed" : (work.UserStatus.ToString().ToLower());
             
            return new
            {
                
                work.Id,
                Number =work.Number.ToString(),
                work.StartedOn,
                StartedBy = work.Starter?.Name,
                Name="work",
                IsEmpty = !(work.Jobs.Any(x=>x.Products.Any()) || work.Offers.Any(x=>x.Products.Any())), // todo optimize?
                ClientId = work.Client?.Id,
                ClientName = work.Client?.Name,
                ClientAddress = work.Client?.Address?.ToString(),//TODO
                ClientEmail = work.Client?.CurrentEmail,
                ClientPhone = work.Client?.Phone,
                VehicleId = work.Vehicle?.Id,
                VehicleProducer = work.Vehicle?.Producer,
                VehicleModel = work.Vehicle?.Model,
                VehicleVin = work.Vehicle?.Vin,
                VehicleRegNr = work.Vehicle?.RegNr,
                work.Notes,
                work.Odo,
                Mechanics = work.Mechanics.ToList().Select(x => new { x.Id, x.Name }).ToArray(),
                Status= status,
                Issuance = work.Invoice is not null ? 
                  new WorkIssuanceDto( work.Invoice.SentOn,work.Invoice.IssuedOn,work.Invoice.Issuer.Name,work.Invoice.Email,work.Invoice.Number,work.Invoice.DueDays,work.Invoice.IsPaid)
                  : null
            };
        }

        [HttpGet("{id}/activities/{currentId?}")]
        public async Task<dynamic> Activities(Guid id,Guid? currentId) 
            
        {
            var activities  = session.Connection.Query<ActivityDto>(
                                @"select a.id,ordernr||'' as number,startedon,name,firstname||' '||lastname as startedby,notes,isvehiclelinesonpricing,isempty from ( 
                                       select 
									       id,
										   ordernr,
										   startedon,
										   starterId,'offer' as name,
										   notes,
										   isvehilelinesonestimate as isvehiclelinesonpricing,
										   (offer.notes IS NULL OR length(offer.notes)=0) 
										    and not   exists(select * from domain.productoffered where offerid = offer.id)
											and not exists(select * from domain.serviceoffered where offerid = offer.id) AS isempty 
										   from domain.offer   where workid = @workid
										   
                                        union all
                                       select 
									      id,
										  ordernr,
										  startedon,
										  starterId,
										  'repairjob' as name,
										  notes,
										  true as isvehiclelinesonpricing,
										    (repairjob.notes IS NULL OR length(repairjob.notes)=0) 
										    and not   exists(select * from domain.productinstalled where repairjobid = repairjob.id)
											and not exists(select * from domain.serviceperformed where repairjobid = repairjob.id) AS isempty 
										 from domain.repairjob   where workid = @workid
                                   ) a
                                   inner join domain.employee e on e.id = a.starterid order by startedon desc", new { workid = id }).ToList();
            if(!activities.Any()) 
            {
                return new { };
            }
            var current  = currentId.GetValueOrDefault()!=Guid.Empty ? activities.Single(x=>x.Id == currentId) : activities.First() ;

            var isRepairJob = current.Name == "repairjob";
            IEnumerable<Product> products =  isRepairJob ?
                    session.QueryOver<ProductInstalled>().Where(x => x.Job.Id == current.Id).List().Cast<Product>() :
                     session.QueryOver<ProductOffered>().Where(x => x.Offer.Id == current.Id).List().Cast<Product>();
             
            return new
            {
                Items = activities,
                Current = new
                {
                    Id = current.Id,
                    Notes = current.Notes,
                    IsVehicleLinesOnPricing = current.IsVehicleLinesOnPricing,
                    Products = products.OrderBy(x => x.Jnr).Select(ToDto).ToArray(),
                    PriceSummary = PriceSummary.CalculatePriceSummary(await GetVatRateaAsync(), products)
                }
            };

        }
        private async Task<int> GetVatRateaAsync()
        {
            var pricingOptions = await tenantConfigService.GetPricingAsync();
            return pricingOptions.Invoice.VatRate;
        }

        [HttpPost]
        public OkObjectResult Post([FromBody]PostOrPutWork model)
        {

            var starter = session.Get<Employee>(this.EmployeeId());
            var work = Work.Start(
                numberProviderFactory,
                starter,
                repository.Get<Client>(model.ClientId.GetValueOrDefault(), false),
                 repository.Get<Vehicle>(model.VehicleId.GetValueOrDefault(), false),
                 model.StartWithOffer ? null : model.Description,
                 model.Odo);

            if (model.AssignedTo != null) work.Assign(model.AssignedTo.Select(x => repository.Get<Employee>(x)).ToArray());
             
            var offer = default(Offer);
            var repairJob = default(RepairJob);
            if (model.StartWithOffer)
            {
                offer = work.CreateOffer(starter, model.Description);
            }
            else
            {
                repairJob = work.StartRepairJob(starter, model.Description);
            }

            repository.Add(work);

            var activityId = model.StartWithOffer ? offer.Id : repairJob.Id;

            return Ok(new
            {
                WorkId = work.Id,
                ActivityId = activityId
            });
        }



        [HttpPut("{id}")]
        public virtual OkObjectResult Put(Guid id, [FromBody] PostOrPutWork model)
        {
            var work = repository.Get<Work>(id);
            if (model.ClientId is not null)
            {
                var client = repository.Get<Client>(model.ClientId.Value);
                work.IsFor(client);
            }
            else
            {
                work.IsForPrivateClient();
            }

            if (model.VehicleId is not null)
            {
                var vehicle = repository.Get<Vehicle>(model.VehicleId.Value);
                work.DoneOn(vehicle);
            }
            else
            {
                work.WithoutVehicle();
            }
            work.WithNotes(model.Description);
            work.Assign(model.AssignedTo == null ? Enumerable.Empty<Employee>().ToArray() : model.AssignedTo.Select(x => repository.Get<Employee>(x)).ToArray());

            work.Changed();//well, can i use dry here
            repository.Update(work);


            return Ok(work.Id);
        }


        [HttpDelete]
        public OkResult Delete([FromBody] Guid[] ids)
        {
            foreach (var id in ids)
            {
                var dObj = repository.Get<Work>(id); 
                if(dObj.Offers.Any(x=>x.Estimate!=null && x.Estimate.SentOn != null)) 
                {
                    throw new UserException("Cannot delete work, it contains an offer sent to a client.");
                }
                
                if(dObj.Invoice!=null && dObj.Invoice.SentOn != null)
                {
                    throw new UserException("Cannot delete work, it contains an invoice sent to a client.");
                }

                repository.Delete(dObj);
            }
            return Ok();
        }
        //todo move queries out refract
        [HttpGet("page")]
        public PagedResult<WorkPage> GetPage(
            string searchText, 
            string orderby, 
            int limit, int offset, bool desc, 
            string status,
            string issued,
            string saleable,
             DateTime? workForm,
            DateTime? workTo,
            DateTime? invoiceFrom, 
            DateTime? invoiceTo )
        {
            var onlyIssued = issued == "on" ;
            var clientId  = Request.Query["clientiId[value]"].FirstOrDefault();
            var vehicleId = Request.Query["vehicleId[value]"].FirstOrDefault();
            orderby = onlyIssued? "i.number": "w.changedon";

             

            string pgDate(DateTime? date) { return date.Value.ToString("s", cultureUS); }
            desc = true;
            var query = repository
                 .PageQuery<WorkPage>(orderby, limit, offset, desc);

           
            if (!onlyIssued)
            {
                query.Where("w.invoiceid is null");
            } 

            if (!string.IsNullOrWhiteSpace(status))
            {
                 
                if (status == "inprogress") query.Where($" w.userstatus = '{WorkStatus.InProgress}'");
                else if (status == "closed") query.Where($" w.userstatus = '{WorkStatus.Closed}'");

                else if (status == "overdue") 
                    query.Where(@"i.ispaid = false and (ip.issuedon + i.duedays * interval '1 day' <=  current_timestamp)");

            }
          
            if (clientId  != null) query.Where($"w.clientid = '{clientId }'");
            if (vehicleId != null ) query.Where($"w.vehicleid = '{vehicleId}' ");
            if (workForm is not null || workForm is not null)
            { 
                var dateRestriction = @" work.startedon {0})";
                if (invoiceTo is null)
                {
                    dateRestriction = string.Format(dateRestriction, $" >= '{pgDate(invoiceFrom)}'");
                }
                else if (invoiceFrom is null)
                {
                    dateRestriction = string.Format(dateRestriction, $" < '{pgDate(invoiceTo)}'");
                }
                else
                {
                    dateRestriction = string.Format(dateRestriction, $" between '{pgDate(invoiceFrom)}' and '{pgDate(invoiceTo)}' ");
                }
                query.Where(dateRestriction);
            }
            if (invoiceFrom is not null || invoiceTo is not null)
            {
               
                var dateRestriction = @"ip.issuedon {0}";
                if (invoiceTo is null)
                {
                    dateRestriction = string.Format(dateRestriction, $" >= '{pgDate(invoiceFrom)}'");
                }
                else if (invoiceFrom is null)
                {
                    dateRestriction = string.Format(dateRestriction, $" < '{pgDate(invoiceTo)}'");
                }
                else
                {
                    dateRestriction = string.Format(dateRestriction, $" between '{pgDate(invoiceFrom)}' and '{pgDate(invoiceTo)}' ");
                }
                query.Where(dateRestriction);
            }
            if (!string.IsNullOrWhiteSpace(saleable))
            {
                var tokens = new WildcardTokens(saleable).AllTokens();
                
                var productTokens = string.Join(" and ", tokens.Select(word => $"concat_ws(' ',p.code,s.name) ilike '%{word}%'"));
                var restriction = onlyIssued ?
$@" exists (select * from domain.productinstalled p 
                        inner join domain.saleable s on s.id = p.id 
                        inner join domain.repairjob rj on rj.id = p.repairjobid
                        where rj.workid = w.id and {productTokens}) ": 
$@" exists (select * from domain.productoffered p 
                        inner join domain.saleable s on s.id = p.id 
                        inner join domain.offer offer on offer.id= p.offerid
                        where offer.workid = w.id and {productTokens}) ";
                 
                query.Where(restriction);
            }


            var issuanceSql =
$@"json_build_object(
	'invoiceNumber',i.number, 
	'isPaid',i.ispaid, 
	'dueDays',i.duedays,
	'issuedOn',ip.issuedon,
    'issuedBy',concat_ws(' ',ii.firstname,ii.lastname) ,
	'sentOn', ip.senton,
	'receiverEmail',ip.email
	) as issuance";
var offerIssuanceSql =
            $@"(select  
	   json_build_object(
	'id',o.id,
	'number',e.number,
	'acceptedOn',o.acceptedOn,
	'acceptedBy',concat_ws(' ',acp.firstname,acp.lastname),
	'sentOn', p.senton,
	'issuedOn',p.issuedon,
	'issuedBy',concat_ws(' ',emp.firstname,emp.lastname) ,  
	'receiverEmail',p.email
	)   from domain.offer o 
	inner join domain.pricing p on p.id = o.estimateid
	inner join domain.estimate e on e.id = o.estimateid
	inner join domain.employee emp on emp.id = p.issuerid
	left join domain.employee acp on acp.id = o.acceptorid
	where page.numberOfOffers = 1 and o.workid = page.id) as offerissuance";

            var extraJoins = string.Empty;
            if (onlyIssued)
            {
                extraJoins =
@"inner join domain.invoice i on i.id = w.invoiceid
inner join domain.pricing ip on ip.id = i.id
inner join domain.employee ii on ii.id = ip.issuerid";
            }

           
            return
               query
                 .FilterBy(searchText)
                 .SearchFields(
@"concat_ws(' ', w.number::text,p.firstname,p.lastname,l.name, v.regnr, v.vin,
	array_to_string((select array_agg(e.number)   from domain.offer o
	inner join domain.estimate e on e.id = o.estimateid
	where workid = w.id ),'/ '),
	(select number from domain.invoice where id = w.invoiceid))")

                 .SelectSql(
$@"select *, 
  case 
    when invoiceid is not null then 'completed'
    else LOWER(userstatus)
  end as status
   {(!onlyIssued? ","+offerIssuanceSql:string.Empty)}
from (
 select
   w.invoiceid, 
   w.id,
   w.userstatus,
   w.number as worknr,   
   w.startedon ,  
	{(onlyIssued?issuanceSql: "(select count(*) from domain.offer o where o.workid = w.id)  as numberOfOffers")}, 
    {(onlyIssued ? string.Empty: "exists (select * from domain.repairjob r where r.workid = w.id)  as hasRepairs,")} 
    w.clientid,
    concat_ws(' ',p.firstname,p.lastname,l.name) as clientname,
    w.vehicleid,
    v.regnr, 
	(select string_agg(concat_ws(' ',m.firstname, m.lastname ),'/ ') 
	   from domain.assignment a 
		inner join domain.employee m on  a.mechanicid = m.id and a.workid = w.id
		) as mechanicnames,
	w.notes  
     from domain.work w
         {extraJoins}
	  left join domain.legalclient l on l.id =  w.clientid
      left join domain.privateclient p on p.id = w.clientid 
	  left join domain.vehicle v on v.id = w.vehicleid
         {query.UseWhereRestriction(false).GetWhereRestriction()}
	     {query.UsePagingRestriction(false).GetPagingRestriction()} 
) page").ToResult();
        }
        
          
        [HttpGet("offer/{offerId}/productsorservices")]
        public  OkObjectResult  GetProductsAndServicesOfAnOffer(Guid offerId) 
        {
             
            var products = session.QueryOver<ProductOffered>()
                  .Where(x => x.Offer.Id == offerId)
                  .List();
            return Ok(products.OrderBy(x => x.Jnr).Select(ToDto).ToArray());
        }

        [HttpPut("offer/{offerId}/productsorservices")]
        public OkObjectResult PutProductsOrServicesOfAnOffer(Guid offerId, [FromBody] PutProductOrService[] model)
        {
            var offer = session.Get<Offer>(offerId);

            var products = model.Select((x, i) => new ProductOffered(offer,  Convert.ToInt16(i + 1)  , x.Code, x.Name, x.Quantity, x.Unit, x.Price, x.Discount, x.Id)).ToArray();

            offer.With(products);

            offer.Work.Changed();//well, can i use dry here
            repository.Update(offer.Work);

            session.Update(offer);
            session.Flush();
             
            return Ok(offerId);

        }

        [HttpGet("repairjob/{jobId}/productsorservices")]
        public OkObjectResult GetProductsOrServicesOfRepairJob(Guid jobId)
        {
            var products = session.QueryOver<ProductInstalled>()
                  .Where(x => x.Job.Id == jobId)
                  .List();
           
            return Ok(products.OrderBy(x => x.Jnr).Select(ToDto).ToArray());

        }
        [HttpPut("repairjob/{jobId}/productsorservices")]
        public OkObjectResult PutProductsOrServicesOfRepairJob(Guid jobId, [FromBody] PutProductOrService[] model)
        {
            var job = session.Get<RepairJob>(jobId);

            var products = model.Select((x, i) => new ProductInstalled(job, Convert.ToInt16(i + 1), x.Code, x.Name, x.Quantity, x.Unit, x.Price, x.Discount, x.Id)).ToArray();

            job.With(products);

            job.Work.Changed();//well, can i use dry here
            repository.Update(job.Work);

            session.Update(job);
            session.Flush();
            return Ok(jobId);

        }

        [HttpPut("{id}/status/{status}")]
        public OkResult PutStatus(Guid id, string status)
        {
            var work = session.Get<Work>(id);

            var newWorkStatus = (WorkStatus)Enum.Parse(typeof(WorkStatus), status);

            work.ChangeState(newWorkStatus);
            if(newWorkStatus!= WorkStatus.Closed)
            {
                work.Changed(); //special case, we don't want closed to appear as changed.. changed work is displayd first
            }
            
            session.Update(work);
            return Ok();
        }

        [HttpPut("{id}/offer/{offerNumber}")]
        public OkResult PutOffer(Guid id, short offerNumber, [FromBody] PutActivityDto model)
        {
            var work = session.Get<Work>(id);
            
            var offer = work.Offers.Single(x => x.OrderNr == offerNumber);

            offer.With(model.Notes);
            offer.DisplayVehicleOnEstimate(model.IsVehicelLinesOnPricing);

            work.Changed();
            session.Update(work);
            session.Update(offer);
            return Ok();
        }

        [HttpPut("{id}/repairjob/{jobNumber}")]
        public OkResult PutRepairJob(Guid id, short jobNumber, [FromBody] PutActivityDto model) //make an activity controller?
        {
            var work = session.Get<Work>(id);

            var job = work.Jobs.Single(x => x.OrderNr == jobNumber);

            job.With(model.Notes);
            // offer.DisplayVehicleOnEstimate(model.IsVehicleLinesOnEstimate);todo?
            work.Changed();
            session.Update(work);
            session.Update(job);
            return Ok();
        }


        [HttpPut("{id}/invoice/paid")]
        public OkResult InvoicePaid(Guid id,   [FromBody] bool isPaid)
        {
            var work = session.Get<Work>(id);
            work.Invoice.MarkPaid(isPaid);

            work.Changed();
            session.Update(work);
            session.Update(work.Invoice);
            return Ok();
        }

        [HttpPut("{id}/invoice/send")]
        public async Task<OkResult> SendInvoice(Guid id, [FromBody] SendPricingDto model)
        {
            var work = session.Get<Work>(id);
            await work.Invoice.Send(pricingSender, model.EmailAddress);//model.displayname

            work.Changed();
            session.Update(work);
            session.Update(work.Invoice);
            return Ok();
        }
        [HttpPut("estimate/send/{offerId}")]
        public async Task<OkResult> SendEstimate( Guid offerId, [FromBody] SendPricingDto model)
        {
            var offer = session.Get<Offer>(offerId); 
            await offer.Estimate.Send(pricingSender, model.EmailAddress);//model.displayname
            offer.Work.Changed();
            session.Update(offer.Work);
            session.Update(offer.Estimate);
            return Ok();
        }
        [HttpPut("{id}/invoice/issue")]
        public async Task<OkResult> IssueInvoice(Guid id,[FromBody]  IssueInvoiceDto model)
        {
            var work = session.Get<Work>(id);
            var issuer = this.Employee();

            work.GenerateInvoice(numberProviderFactory, await GetVatRateaAsync(), model.PaymentType, model.DueDays, issuer);

            session.Save(work.Invoice);

            if (model.SendClientEmail)
            {
                await work.Invoice.Send(pricingSender, model.ClientEmail);
            }
            work.Changed();
            session.Update(work);
            return Ok();
        }

        [HttpPost("{id}/makecopy")]
        public OkObjectResult MakeCopy(Guid id)
        {
            var work = session.Get<Work>(id);
            var newWork = work.CreateCopy(this.numberProviderFactory.GetNumberProvider<Work>().Next(), this.Employee());
            session.Save(newWork);
            return Ok(newWork.Id); 
        }
         
        [HttpPut("{id}/estimate/issue/{offerNumber}")]
        public async Task<OkObjectResult>  IssueEstimate(Guid id, int offerNumber, [FromBody]IssuePricingDto model)
        {
            var work = session.Get<Work>(id);
             
            var offer = work.Offers.Single(x => x.OrderNr == offerNumber);
            var issuer = this.Employee();
              
            var offerIssued = await work.Issue(offer,pricingSender, await GetVatRateaAsync(), issuer,model.ShowVehicleOnPricing, model.SendClientEmail, model.ClientEmail);

            work.Changed();
            session.Update(work);

            return Ok(offerIssued.Id);
        }
 

        [HttpPut("{id}/invoice/delete")]
        public OkResult CancelInvoice(Guid id)
        {
       
            var work = session.Get<Work>(id);
            var invoice = work.Invoice;
            work.DeleteInvoice(numberProviderFactory);

            work.Changed();
            session.Update(work);
            session.Delete(invoice);
            DeletePdf(invoice);
            return Ok();
        }

        private void DeletePdf(Pricing pricing) 
        {
            var pdfLocalFile = new FileInfo(Path.Combine(configuration["PdfDirectory"], pricing.GetFileName()));
            if (pdfLocalFile.Exists) 
            {
                pdfLocalFile.Delete();
            }
        }

        
        [HttpPut("{id}/estimate/{offerNumber}/accepted/{targetJobNumber?}")]
        public OkObjectResult EstimateAccepted(Guid id, short offerNumber,short? targetJobNumber, [FromBody]string notes)
        {
            var work = session.Get<Work>(id);
            var offer = work.Offers.Single(x => x.OrderNr == offerNumber);
            var acceptor = this.Employee();
             
            var job = offer.Accepted(targetJobNumber, notes,acceptor);

            work.Changed();
            session.Update(work);
            session.Flush();
            return Ok(job.Id);
        }

         
        [HttpDelete("{id}/offer/{offerNumber}")]
        public OkResult DeleteOffer(Guid id, int offerNumber)
        {
            var work = session.Get<Core.Domain.Work>(id);
            var offer = work.Offers.Single(x => x.OrderNr == offerNumber);
            work.Remove(offer);
            work.Changed();
            session.Update(work);
            session.Flush();
            return Ok();
        }
        [HttpDelete("{id}/repairjob/{jobNumber}")]
        public OkResult DeleteRepairJob(Guid id, int jobNumber)
        {
            var work = session.Get<Core.Domain.Work>(id);
            var job = work.Jobs.Single(x => x.OrderNr == jobNumber);
            work.Remove(job);
            work.Changed();
            session.Update(work);
            session.Flush();
            return Ok();
        }

        [HttpPost("{id}/repairjob")]
        public OkObjectResult StartRepairJob(Guid id, [FromBody] string notes)
        {

            var work = session.Get<Core.Domain.Work>(id);
            var job = work.StartRepairJob(this.Employee(), notes);
            work.Changed();
            session.Update(work);
            session.Flush();
            return Ok(job.Id);
        }
        [HttpPost("{id}/offer")]
        public OkObjectResult MakeOnOffer(Guid id,[FromBody] string notes)
        {
            
             var work = session.Get<Core.Domain.Work>(id);
             var offer =  work.CreateOffer(this.Employee(), notes);
            work.Changed();
            session.Update(work);
            session.Flush();
            return Ok(offer.Id);
        }
         
        private dynamic ToDto(Product saleable) 
        { 
            return new
            {
                saleable.Id,
                saleable.Name,
                saleable.Quantity,
                saleable.Unit,
                saleable.Price,
                saleable.Discount,
                saleable.Code
            };
        }

        private Employee Employee() 
        {
           return session.Get<Employee>(this.EmployeeId());
        }
    }
}
