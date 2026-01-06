using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Carmasters.Core.Application.Extensions.DependencyInjection
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsToApp(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", policy =>
                { 
                    var corsMode = configuration.GetValue<string>("Cors:Mode") ?? "restricted";

                    if (corsMode.ToLower() == "open")
                    {
                        policy.AllowAnyOrigin()
                             .AllowAnyHeader()
                             .AllowAnyMethod();
                    }
                    else
                    {
                        var appHost = configuration.GetSection("Cors:AppHost").Value;
                        if (string.IsNullOrWhiteSpace(appHost)) throw new Exception("Cors host not configured.");

                        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == appHost);
                        policy.WithHeaders("Content-Type", "Authorization");
                        policy.WithMethods("GET", "PUT", "POST", "DELETE", "OPTIONS");
                    }
                });
            });
        }
    }
}
