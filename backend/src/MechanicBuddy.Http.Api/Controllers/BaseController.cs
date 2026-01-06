using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Carmasters.Core;
using Carmasters.Core.Application;
using Carmasters.Core.Domain;
using Carmasters.Core.Repository.Postgres;
using Carmasters.Http.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers
{

    /// <summary>
    /// only httpget and httpost get and create
    /// </summary>
    /// <typeparam name="MODEL"></typeparam>
    /// <typeparam name="DOMAINOBJECT"></typeparam>
    public abstract class BaseController<MODEL, DOMAINOBJECT> : ControllerBase where DOMAINOBJECT : GuidIdentityEntity
    {

        protected readonly IRepository repository;
        protected readonly IMapper mapper;

        protected BaseController(IRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }
        [Authorize(Policy = "ServerSidePolicy")]
        [HttpGet("{id}")]
        public virtual ActionResult Get(Guid id)
        {
            var emp = repository.Get<DOMAINOBJECT>(id, false);
            var model = Map(emp);
            AfterGet(model, emp);
            return new JsonResult(model);
        }

        protected virtual MODEL Map(DOMAINOBJECT entity)
        {
            return mapper.Map<MODEL>(entity);
        }

        [HttpPost]
        public virtual ActionResult Post(MODEL model)
        {
            var domainObj = CreateFrom(model);
            repository.Add(domainObj);
            AfterSaved(model, domainObj);
            return new JsonResult(domainObj.Id);
        }
        protected virtual void AfterGet(MODEL model, DOMAINOBJECT domainObj)
        {
        }
        protected virtual void AfterSaved(MODEL model, DOMAINOBJECT domainObj)
        {
        }
        protected virtual void AfterUpdated(MODEL model, DOMAINOBJECT domainObj)
        {
        }
        [Authorize(Policy = "ServerSidePolicy")]
        [HttpPut("{id}")]
        public virtual ActionResult Put(Guid id, MODEL model)
        {
            var domainObj = repository.Get<DOMAINOBJECT>(id);
            Edit(domainObj, model);
            repository.Update(domainObj);
            AfterUpdated(model, domainObj);
            return new JsonResult(id);
        }

        [Authorize(Policy = "ServerSidePolicy")]
        [HttpDelete]
        public OkResult Delete([FromBody] Guid[] ids)
        {
            foreach (var id in ids)
            {
                var dObj = repository.Get<DOMAINOBJECT>(id);
                repository.Delete(dObj);
            }
            return Ok();
        }

        protected virtual void BeforeDelete(DOMAINOBJECT domainObj) { }

        protected virtual void Edit(DOMAINOBJECT entity, MODEL model)
        {
            throw new NotSupportedException();
        }
        protected virtual DOMAINOBJECT CreateFrom(MODEL model)
        {
            throw new NotSupportedException();
        }

    }
}
