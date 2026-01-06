
using Carmasters.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Carmasters.Core.Application.Extensions.DependencyInjection
{
    public static class DemoExtensions
    {
        public static IServiceCollection AddDemoSetupServices(this IServiceCollection services)
        {
            services.AddScoped<IDemoSetupService, DemoSetupService>(); 
            return services;
        }
    }
}