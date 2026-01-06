using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Carmasters.Core.Domain
{  
    public  interface IRepository {
         
        void Add<T>(T entity) where T : class;
        T Get<T>(Guid id, bool failWhenNotFound = true) where T : class;
        void Update<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;

        IDbConnection GetConnection();
    }

  
}