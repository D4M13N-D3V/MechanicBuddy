using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Carmasters.Core.Application.Database
{
    public class DbConnectionProvider
    {
        public const string ItemKey = "dbConnectionScope";
        private readonly IHttpContextAccessor httpContextAccessor;
        public DbConnectionProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        private IDictionary<object, object> Items => httpContextAccessor.HttpContext.Items;

        public DbConnection GetConnection()
        {
            if (Items[ItemKey] == null) throw new Exception("servicescope not available.");
            var connection = ((IServiceScope)Items[ItemKey]).ServiceProvider.GetRequiredService<DbConnection>();
            return connection;
        }
    }
}
