using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carmasters.Core.Application.Authorization;
using Carmasters.Http.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NHibernate.Criterion;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Carmasters.Core.Application.Documentation
{
    public class SecurityTrimming : IDocumentFilter
    { 
        private readonly IHttpContextAccessor contextAccessor;
        private string[] usersAllowed;

        public SecurityTrimming(IHttpContextAccessor contextAccessor,IConfiguration configuration)
        { 
            this.contextAccessor = contextAccessor;
            usersAllowed = configuration.GetSection("SwaggerOptions:Users").Get<string[]>();
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var identity = contextAccessor.HttpContext?.User?.Identity;

            var userAllowed = identity is not null && identity.IsAuthenticated && identity.Name.IsIn(usersAllowed);

            if (!userAllowed) 
            {
                var keys = context.SchemaRepository.Schemas.Keys;
                foreach (var key in keys)
                {
                    if (key == nameof(LoginDto)) continue;
                    if (key == nameof(DemoSetupRequest)) continue;
                    if (key == nameof(DemoSetupResponse)) continue;
                    context.SchemaRepository.Schemas.Remove(key);
                }

                var route1 = "api/Users/authenticate";
                var routeDescription1 = context.ApiDescriptions.Single(x => x.RelativePath == route1);
                var swaggerPath1 = swaggerDoc.Paths["/" + route1];

                var route2 = "api/Demo/setup";
                var routeDescription2 = context.ApiDescriptions.Single(x => x.RelativePath == route2);
                var swaggerPath2 = swaggerDoc.Paths["/" + route2];
                 
                swaggerDoc.Paths.Clear(); 
                swaggerDoc.Paths.Add("/" + route1, swaggerPath1);
                swaggerDoc.Paths.Add("/" + route2, swaggerPath2);
            } 
        }
         
    }
}
