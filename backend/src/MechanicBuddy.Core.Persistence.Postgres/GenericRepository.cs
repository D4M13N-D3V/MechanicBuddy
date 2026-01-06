using Carmasters.Core.Domain;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data;
using NHibernate;
using NHibernate.Criterion;
using System;

namespace Carmasters.Core.Repository.Postgres
{
    public class GenericRepository : IRepository
    {
        protected readonly ISession context;

        [DebuggerStepThrough]
        public GenericRepository(ISession context) {
            this.context = context;  
        }
         
        public T Get<T>(Guid id,bool failWhenNotFound = true) where T : class
        {
            var entity= context.Get<T>(id);
            if (entity == null && failWhenNotFound) {
                throw new EntityNotFoundException(typeof(T).Name);
            } 
            return entity;
        }
           
        public virtual void Add<T>(T entity)  where T: class
        {
            context.Save(entity); 
        } 
        public virtual void Update<T>(T entity) where T : class
        {
            context.Update(entity);
        } 
        public virtual void Delete<T>(T entity) where T : class
        {
            context.Delete(entity);
        }

        public IDbConnection GetConnection()
        {
            return context.Connection;
        }
    }
}