using MechanicBuddy.Core.Application.Configuration;
using MechanicBuddy.Core.Application.Database;
using MechanicBuddy.Core.Application.Services;
using MechanicBuddy.Core.Domain;
using MechanicBuddy.Core.Persistence.Postgres;
using MechanicBuddy.Core.Persistence.Postgres.NHibernate;
using MechanicBuddy.Core.Persistence.Postgres.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NHibernate;
using NHibernate.Mapping;
using System.Data.Common;
using System.Reflection;

namespace MechanicBuddy.Core.Repository.Postgres
{
    public static class DependencyInjectionExtensions
    {
        static object lockObj = new object();
        public static IServiceCollection AddPersistanceServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder();
            var options = new DbOptions(); configuration.GetSection("DbOptions").Bind(options);
            connectionBuilder.Host = options.Host;
            connectionBuilder.Port = options.Port;
            connectionBuilder.Username = options.UserId;
            connectionBuilder.Password = options.Password;
            connectionBuilder.Database = options.Name;
            var multitenancyEnabled = options.MultiTenancy?.Enabled == true;
            var defaultFactory = default(ISessionFactory);
            var mappingAssemblies = new System.Collections.Generic.List<Assembly>() { typeof(UserDbMapping).Assembly };
            if (multitenancyEnabled)
            {
                connectionBuilder.Database = new MultiTenancyDbName(options, DbKind.Tenancy);
                defaultFactory = NNhibernateFactory.BuildSessionFactory(mappingAssemblies, connectionBuilder.ToString());
            }
            else 
            {
                mappingAssemblies.Add(typeof(WorkMapping).Assembly);
                defaultFactory = NNhibernateFactory.BuildSessionFactory(mappingAssemblies, connectionBuilder.ToString());
            }        

            var appFactory = default(ISessionFactory);
            
            services.AddScoped<IUserRepository>(x => {
                return new UserRepository(x.GetRequiredService<IOptions<DbOptions>>());
            });
            services.AddScoped<NHibernate.ISession>(x =>{

                if (!multitenancyEnabled) return defaultFactory.OpenSession();

                var httpContext = x.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>().HttpContext;
                var user = httpContext?.User;

                if (user?.Identity?.IsAuthenticated == true)
                {
                    if (appFactory == null)
                    {
                        lock (lockObj)
                        {
                            if (appFactory == null) //double if, if anyone was waiting it might have been initialized already
                            {
                                appFactory = NNhibernateFactory.BuildSessionFactory(new System.Collections.Generic.List<Assembly>() { typeof(WorkMapping).Assembly });
                            }
                        }
                    }
                    return appFactory.OpenSession();
                }

                // For anonymous endpoints (like service request submission), use the app factory with domain mappings
                var endpoint = httpContext?.GetEndpoint();
                var allowAnonymous = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null;
                if (allowAnonymous)
                {
                    if (appFactory == null)
                    {
                        lock (lockObj)
                        {
                            if (appFactory == null)
                            {
                                appFactory = NNhibernateFactory.BuildSessionFactory(new System.Collections.Generic.List<Assembly>() { typeof(WorkMapping).Assembly });
                            }
                        }
                    }
                    return appFactory.OpenSession();
                }

                throw new System.Exception("Unable to open database session, user not authenticated.");
            });


            services.AddScoped<IRepository, GenericRepository>();
          
            services.AddScoped<ISequnceNumberProviderFactory, SequenceNumberProviderFactory>();
            services.AddScoped<InvoiceSequenceNumberProvider>();
            services.AddScoped<WorkSequenceNumberProvider>();
            services.AddScoped<EstimateSequenceNumberProvider>(); 
            services.AddScoped<UnitOfWorkAspect>();
            services.AddSingleton<DbConnectionProvider>();
            services.AddScoped<DbConnection>(x => new Npgsql.NpgsqlConnection());
            services.AddSingleton<MultiTenancyConnectionDriver>();
            services.AddSingleton<DatabaseBackup>();
            services.AddScoped<ITenancyRepository, TenancyRepository>();
            return services;
        }
    }
     
}