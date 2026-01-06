using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AutoMapper;
using Carmasters.Core.Application;
using Carmasters.Core.Application.Authorization;
using Carmasters.Core.Application.Database;
using Carmasters.Core.Application.Extensions;
using Carmasters.Core.Application.Model;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Domain;
using Carmasters.Core.Repository.Postgres;
using Carmasters.Http.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Mapping;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : BaseController<EmployeeDto, Employee>
    {
        private readonly IUserRepository userRepository;
        private readonly ISession session;

        public EmployeesController(IUserRepository userRepository, IRepository repository, IMapper mapper, ISession session) : base(repository, mapper)
        {
            this.userRepository = userRepository;
            this.session = session;
        }

        [HttpGet]
        public dynamic Get() 
        {
            return session.Query<Employee>().Select(x => new { 
               x.Id,
               x.Name
            }).ToList();
        }

        [HttpGet("page")]
        public PagedResult<EmployeeDto> GetPage(string searchText, string orderby, int limit, int offset, bool desc)
        {
            var page =
                repository
                  .PageQuery<EmployeeDto>(orderby, limit, offset, desc)
                  .FilterBy(searchText)
                  .SearchFields("concat_ws(' ',firstname,lastname,email,phone)") //u.username
                  .SelectSql(@"select employee.*,'' as username  from domain.employee ") //left join public.user u on u.employeeid = employee.id
                  .ToResult();

         
            var users =  userRepository.GetAllByTenant(this.TenantName());

            foreach (var entry in page.Items)
            {
                var user = users.Where(x => (Guid)x.Id.EmployeeId == entry.Id).SingleOrDefault();
                if (user != null)
                {
                    entry.UserName = user.UserName;
                }
            }

            return page;
        }

        protected override void AfterGet(EmployeeDto model, Employee domainObj)
        {
            base.AfterGet(model, domainObj);
            //check user 
            var userName = GetUser(domainObj)?.UserName;
            model.UserName = userName;
        }

        protected override void AfterSaved(EmployeeDto model, Employee domainObj)
        {
            base.AfterSaved(model, domainObj);
            if (!string.IsNullOrWhiteSpace(model.UserName))
            {
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    throw new ArgumentException("Parool sisestamata.");
                }
                var user = userRepository.GetBy(model.UserName);
                if (user != null) throw new ArgumentException("Seda kasutajanime ei saa kasutada.");

                var newUser = NewUser(model, domainObj);
                userRepository.Update(newUser);
            }
        }
         
        private User GetUser(Employee domainObj)
        {
            return userRepository.GetBy(new UserIdentifier(this.TenantName(), domainObj.Id));
        }
        private User NewUser(EmployeeDto model, Employee domainObj)
        {
            return new User(model.UserName, PasswordHasher.getHash(model.Password), model.Email, false, null, new UserIdentifier(this.TenantName(), domainObj.Id));
        }
        protected override Employee CreateFrom(EmployeeDto model)
        {
            var employee = new Employee(model.FirstName, model.LastName, DateTime.Now, model.Phone, model.Email, model.Proffession, model.Description);

            return employee;
        }

        protected override void Edit(Employee employee, EmployeeDto model)
        {
            employee.Change(model.FirstName, model.LastName, model.Phone, model.Email, model.Proffession, model.Description);
        }
    }
}
