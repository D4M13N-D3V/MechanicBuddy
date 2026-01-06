using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Carmasters.Core.Application.RateLimiting
{
    public class DemoRateLimitStrategy : StandardRateLimitStrategy
    {
        public DemoRateLimitStrategy(IDistributedCache cache) : base(cache)
        {
        }

        public override string GenerateRateLimitKey(HttpContext context, LimitRequests limitAttribute)
        {
            return $"demo_rate_limit_{GetClientIpAddress(context)}";
        }

        public override async Task HandleRateLimitExceeded(HttpContext context, LimitRequests limitAttribute, string key)
        {
            var clientStat = await _cache.GetCacheValueAsync<ClientStatistics>(key);
            var timeRemaining = clientStat.LastSuccessfulResponseTime.AddSeconds(limitAttribute.TimeWindow) - DateTime.UtcNow;

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Rate limit exceeded",
                message = $"You can only create one demo instance per day. Please try again in {timeRemaining.Hours} hours and {timeRemaining.Minutes} minutes.",
                retryAfter = (int)timeRemaining.TotalSeconds
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }

        public override async Task UpdateRateLimitStatistics(string key, LimitRequests limitAttribute)
        {
            var clientStat = await _cache.GetCacheValueAsync<ClientStatistics>(key);

            if (clientStat != null)
            {
                clientStat.LastSuccessfulResponseTime = DateTime.UtcNow;

                if (clientStat.NumberOfRequestsCompletedSuccessfully == limitAttribute.MaxRequests)
                    clientStat.NumberOfRequestsCompletedSuccessfully = 1;
                else
                    clientStat.NumberOfRequestsCompletedSuccessfully++;

                // Demo rate limits have longer expiration - 25 hours
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(25)
                };

                await _cache.SetCacheValueAsync(key, clientStat, options);
            }
            else
            {
                var clientStatistics = new ClientStatistics
                {
                    LastSuccessfulResponseTime = DateTime.UtcNow,
                    NumberOfRequestsCompletedSuccessfully = 1
                };

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(25)
                };

                await _cache.SetCacheValueAsync(key, clientStatistics, options);
            }
        }
    }
}
