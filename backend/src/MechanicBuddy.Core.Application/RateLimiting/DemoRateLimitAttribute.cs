using System;

namespace Carmasters.Core.Application.RateLimiting
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DemoRateLimitAttribute : LimitRequests
    {
        public DemoRateLimitAttribute()
        {
            // Set to 1 request per 24 hours (86400 seconds)
            TimeWindow = 86400;
            MaxRequests = 1;
        }
    }

    /// <summary>
    /// 300 requests per 5‑minute sliding window for one tenant.
    /// Override MaxRequests / TimeWindow on the attribute if you
    /// need finer control per‑endpoint.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class TenantRateLimitAttribute : LimitRequests
    {
        public TenantRateLimitAttribute()
        {
            MaxRequests = 300;      // requests
            TimeWindow = 300;      // seconds (5 min)
        }
    }
}
