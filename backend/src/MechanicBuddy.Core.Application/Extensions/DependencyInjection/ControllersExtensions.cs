using MechanicBuddy.Core.Application.Authorization;
using MechanicBuddy.Core.Application.Database;
using MechanicBuddy.Core.Application.Errors;
using FluentNHibernate.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MechanicBuddy.Core.Application.Extensions.DependencyInjection
{
    public static class ControllersExtensions
    {
        public static IServiceCollection AddControllersWithViewsToApp(this IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<JsonResponseExceptionFilter>();
                options.Filters.Add(typeof(UnitOfWorkAspect));
                 

            });
           
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ServerSidePolicy",
                     policy => policy.RequireRole("Root"));
                  
            });


            services.Configure<ApiBehaviorOptions>(o =>
            {
                o.InvalidModelStateResponseFactory = actionContext => InvalidModelStateJsonResponseFactory.Handle(actionContext);

            });


            return services;
        } 
    }
}
