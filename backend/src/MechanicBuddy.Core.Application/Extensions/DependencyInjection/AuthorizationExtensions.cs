using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Extensions.DependencyInjection
{
	public static class AuthorizationExtensions
    {
    
        public static IServiceCollection AddJwtAuthenticationToApp(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtOptions");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                };
				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
                        var jwt = context.Request.Cookies["jwt_token"];
                        if (!string.IsNullOrWhiteSpace(jwt)) 
                        {
							context.Token = jwt;
						}
						return Task.CompletedTask;
					}
				};
			});
            return services;
        }
    }
}
