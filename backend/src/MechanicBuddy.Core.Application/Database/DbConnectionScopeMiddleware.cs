using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Database
{
    public class DbConnectionScopeMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public DbConnectionScopeMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            this.next = next;
            this.serviceScopeFactory = serviceScopeFactory;
        }
        public async Task Invoke(HttpContext context)
        { 
            try
            {
                var scope = serviceScopeFactory.CreateScope();
                context.Items[DbConnectionProvider.ItemKey] = scope;
                await next(context);
            }
            finally
            {
                if (context.Items[DbConnectionProvider.ItemKey] != null)
                {
                    ((IServiceScope)context.Items[DbConnectionProvider.ItemKey]).Dispose();
                    context.Items[DbConnectionProvider.ItemKey] = null;
                }
            }
        }
    }
}
