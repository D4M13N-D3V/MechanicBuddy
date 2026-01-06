using Carmasters.Core.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Carmasters.Core.Application.Extensions.DependencyInjection
{
    public static class OptionsExtensions
    {
        public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
            services.ConfigureWritable<PricingOptions>(configuration.GetSection("Pricing"));
            services.ConfigureWritable<RequisitesOptions>(configuration.GetSection("Requisites"));
            services.Configure<PricingOptions>(configuration.GetSection("Pricing"));
            services.Configure<RequisitesOptions>(configuration.GetSection("Requisites"));
            services.Configure<SmtpOptions>(configuration.GetSection("SmtpOptions"));
            services.Configure<DbOptions>(configuration.GetSection("DbOptions"));
            return services;
        }

		private static void ConfigureWritable<T>(
		  this IServiceCollection services,
		  IConfigurationSection section,
		  string file = "appsettings.json") where T : class, new()
		{
			services.Configure<T>(section);
			services.AddTransient<IWritableOptions<T>>(provider =>
			{
				var configuration = (IConfigurationRoot)provider.GetService<IConfiguration>();
				var environment = provider.GetService<IHostEnvironment>();
				var options = provider.GetService<IOptionsMonitor<T>>();
				return new WritableOptions<T>(environment, options, configuration, section.Key, file);
			});
		}
	}
}
