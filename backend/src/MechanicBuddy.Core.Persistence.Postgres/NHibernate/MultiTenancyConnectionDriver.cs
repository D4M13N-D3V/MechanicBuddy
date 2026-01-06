using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using Carmasters.Core.Application;
using Carmasters.Core.Application.Configuration;
using Carmasters.Core.Application.Database;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Connection;

namespace Carmasters.Core.Persistence.Postgres.NHibernate
{
     
    public class MultiTenancyConnectionDriver : ConnectionProvider, IMultiTenancyConnectionDriver
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor contextAccessor; 
        private readonly DbConnectionProvider connectionProvider;
        private readonly DbOptions options;

        public MultiTenancyConnectionDriver(IConfiguration configuration, IHttpContextAccessor contextAccessor, DbConnectionProvider serviceProvider) 
        {
            this.configuration = configuration;
            this.contextAccessor = contextAccessor; 
            this.connectionProvider = serviceProvider;
            options = new DbOptions(); configuration.GetSection("DbOptions").Bind(options);
        }
        public override void Configure(IDictionary<string, string> settings)
        {
            settings["connection.connection_string"] = BuildConnectionString();
            base.Configure(settings);
        }
         
        public override DbConnection GetConnection(string connectionString)
        { 
            var connection = connectionProvider.GetConnection(); //disposed by scope
            var principal = contextAccessor.HttpContext.User;
            if (principal == null) new Exception("Current ClaimsPrincipal is null");

            var tenantName = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Spn)?.Value;
            if (string.IsNullOrWhiteSpace(tenantName))
            {
                throw new Exception("Current principal is not valid, spn missing");
            }
            var connectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString); 
            connectionBuilder.Database = new MultiTenancyDbName(options, tenantName); 
            connection.ConnectionString = connectionBuilder.ToString();
            connection.Open();
            return connection;
        } 
        protected override void ConfigureDriver(IDictionary<string, string> settings)
        {
            base.ConfigureDriver(settings);
        }
        public string BuildConnectionString()
        {  
            var connectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder(); 
            connectionBuilder.Host = options.Host;
            connectionBuilder.Port = options.Port;
            connectionBuilder.Database = new MultiTenancyDbName(options, DbKind.Template);
            connectionBuilder.Username = options.UserId;
            connectionBuilder.Password = options.Password;
            return connectionBuilder.ToString();

        } 
    }
}
