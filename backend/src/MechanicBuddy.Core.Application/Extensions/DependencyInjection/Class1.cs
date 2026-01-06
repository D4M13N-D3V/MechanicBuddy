using Carmasters.Core.Application.Services;
using Carmasters.Core.Persistence.Postgres.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Carmasters.Core.Application.Extensions.DependencyInjection
{
    public static class TenantConfigurationExtensions
    {
        public static IServiceCollection AddTenantConfigurationServices(this IServiceCollection services)
        {
            // Register the repository and service
            services.AddScoped<ITenantConfigRepository, TenantConfigRepository>();
            services.AddScoped<ITenantConfigService, TenantConfigService>();

            return services;
        }
    }
}
