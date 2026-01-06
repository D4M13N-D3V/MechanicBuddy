using Microsoft.AspNetCore.Builder;

namespace MechanicBuddy.Core.Application.RateLimiting
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
