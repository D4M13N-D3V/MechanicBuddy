using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MechanicBuddy.Core.Application.Extensions.DependencyInjection
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
                    // Security: validate issuer/audience (must match AppJwtToken.Generate)
                    // and pin the signing algorithm to HS256 to avoid algorithm confusion.
                    ValidateIssuer = true,
                    ValidIssuer = "MechanicBuddy",
                    ValidateAudience = true,
                    ValidAudience = "MechanicBuddy",
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    // small skew tolerance for distributed clocks
                    ClockSkew = TimeSpan.FromSeconds(30)
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
