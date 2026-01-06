using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace Carmasters.Core.Application.RateLimiting
{
    public class RateLimitStrategyFactory
    {
        private readonly IDistributedCache _cache;

        public RateLimitStrategyFactory(IDistributedCache cache)
        {
            _cache = cache;
        }

        public IRateLimitStrategy CreateStrategy(HttpContext context, LimitRequests limitAttribute)
        {
            if (context.User?.Identity?.IsAuthenticated == true &&context.User.HasClaim(c => c.Type == ClaimTypes.Spn))
            {
                return new TenantRateLimitStrategy(_cache);
            }
            if (limitAttribute is DemoRateLimitAttribute)
            {
                return new DemoRateLimitStrategy(_cache);
            }

            return new StandardRateLimitStrategy(_cache);
        }
    }
}
