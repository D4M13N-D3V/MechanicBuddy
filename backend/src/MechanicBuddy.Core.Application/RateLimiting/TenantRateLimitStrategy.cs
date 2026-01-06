using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Carmasters.Core.Application.RateLimiting
{
    /// Counts requests per authenticated tenant (ClaimTypes.Spn).
    /// Falls back to IP key if the claim is missing.
    /// </summary>
    public sealed class TenantRateLimitStrategy : StandardRateLimitStrategy
    {
        public TenantRateLimitStrategy(IDistributedCache cache) : base(cache) { }

        public override string GenerateRateLimitKey(HttpContext ctx,
                                                    LimitRequests attr)
        {
            var tenant = ctx.User?.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.Spn)
                            ?.Value;

            // unauthenticated paths or missing claim – fall back to IP
            if (string.IsNullOrWhiteSpace(tenant))
                return base.GenerateRateLimitKey(ctx, attr);

            // Example key: "/api/work POST_demo‑123"
            return $"{ctx.Request.Path}_{ctx.Request.Method}_{tenant}";
        }
    }
}
