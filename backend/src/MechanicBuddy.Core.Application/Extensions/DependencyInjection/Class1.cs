using MechanicBuddy.Core.Application.Services;
using MechanicBuddy.Core.Persistence.Postgres.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicBuddy.Core.Application.Extensions.DependencyInjection
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
