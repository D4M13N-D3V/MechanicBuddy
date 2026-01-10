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
            var anonymousFactory = default(ISessionFactory);

            // Build anonymous factory for service requests when TenantId is configured
            if (multitenancyEnabled && !string.IsNullOrEmpty(options.MultiTenancy?.TenantId))
            {
                var anonConnectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder();
                anonConnectionBuilder.Host = options.Host;
                anonConnectionBuilder.Port = options.Port;
                anonConnectionBuilder.Username = options.UserId;
                anonConnectionBuilder.Password = options.Password;
                anonConnectionBuilder.Database = new MultiTenancyDbName(options, options.MultiTenancy.TenantId);
                anonymousFactory = NNhibernateFactory.BuildSessionFactory(
                    new System.Collections.Generic.List<Assembly>() { typeof(WorkMapping).Assembly },
                    anonConnectionBuilder.ToString());
            }

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

                // For anonymous endpoints (like service request submission), use the anonymous factory
                var endpoint = httpContext?.GetEndpoint();
                var allowAnonymous = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null;
                if (allowAnonymous && anonymousFactory != null)
                {
                    return anonymousFactory.OpenSession();
                }

                // For shared multi-tenant instances, try to resolve tenant from headers
                // Priority: X-Tenant-ID > X-Forwarded-Host > Host
                string resolvedTenantId = null;

                // 1. Check X-Tenant-ID header (explicit tenant identification)
                if (httpContext?.Request?.Headers?.TryGetValue("X-Tenant-ID", out var tenantIdHeader) == true)
                {
                    // Use FirstOrDefault() as ToString() on StringValues may not work as expected
                    resolvedTenantId = tenantIdHeader.FirstOrDefault();
                }

                // 2. Check X-Forwarded-Host header (set by ingress when proxying)
                if (string.IsNullOrEmpty(resolvedTenantId) &&
                    httpContext?.Request?.Headers?.TryGetValue("X-Forwarded-Host", out var forwardedHost) == true)
                {
                    var host = forwardedHost.FirstOrDefault();
                    if (!string.IsNullOrEmpty(host))
                    {
                        var parts = host.Split('.');
                        if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[0]) && parts[0] != "www" && parts[0] != "api")
                        {
                            resolvedTenantId = parts[0];
                        }
                    }
                }

                // 3. Fall back to Host header
                if (string.IsNullOrEmpty(resolvedTenantId) && httpContext?.Request?.Host.Host != null)
                {
                    var host = httpContext.Request.Host.Host;
                    var parts = host.Split('.');
                    if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[0]) && parts[0] != "www" && parts[0] != "api")
                    {
                        resolvedTenantId = parts[0];
                    }
                }

                // If we have a tenant ID, build session factory for that tenant's database
                if (!string.IsNullOrEmpty(resolvedTenantId))
                {
                    // Build a session factory for this tenant's database
                    var tenantConnectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder();
                    tenantConnectionBuilder.Host = options.Host;
                    tenantConnectionBuilder.Port = options.Port;
                    tenantConnectionBuilder.Username = options.UserId;
                    tenantConnectionBuilder.Password = options.Password;
                    tenantConnectionBuilder.Database = new MultiTenancyDbName(options, resolvedTenantId);

                    var tenantFactory = NNhibernateFactory.BuildSessionFactory(
                        new System.Collections.Generic.List<Assembly>() { typeof(WorkMapping).Assembly },
                        tenantConnectionBuilder.ToString());
                    return tenantFactory.OpenSession();
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