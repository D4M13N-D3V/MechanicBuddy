using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Carmasters.Core.Application.RateLimiting
{
    public class StandardRateLimitStrategy : IRateLimitStrategy
    {
        protected readonly IDistributedCache _cache;

        public StandardRateLimitStrategy(IDistributedCache cache)
        {
            _cache = cache;
        }

        public virtual string GenerateRateLimitKey(HttpContext context, LimitRequests limitAttribute)
        {
            return $"{context.Request.Path}_{GetClientIpAddress(context)}";
        }

        public async Task<bool> IsRateLimitExceeded(string key, LimitRequests limitAttribute)
        {
            var clientStatistics = await _cache.GetCacheValueAsync<ClientStatistics>(key);

            if (clientStatistics != null &&
                DateTime.UtcNow < clientStatistics.LastSuccessfulResponseTime.AddSeconds(limitAttribute.TimeWindow) &&
                clientStatistics.NumberOfRequestsCompletedSuccessfully >= limitAttribute.MaxRequests)
            {
                return true;
            }

            return false;
        }

        public async virtual Task UpdateRateLimitStatistics(string key, LimitRequests limitAttribute)
        {
            var clientStat = await _cache.GetCacheValueAsync<ClientStatistics>(key);

            if (clientStat != null)
            {
                clientStat.LastSuccessfulResponseTime = DateTime.UtcNow;

                if (clientStat.NumberOfRequestsCompletedSuccessfully == limitAttribute.MaxRequests)
                    clientStat.NumberOfRequestsCompletedSuccessfully = 1;
                else
                    clientStat.NumberOfRequestsCompletedSuccessfully++;

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(limitAttribute.TimeWindow * 2)
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
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(limitAttribute.TimeWindow * 2)
                };

                await _cache.SetCacheValueAsync(key, clientStatistics, options);
            }
        }

        public async virtual Task HandleRateLimitExceeded(HttpContext context, LimitRequests limitAttribute, string key)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await Task.CompletedTask;
        }

        public async Task AddRateLimitHeaders(HttpContext context, string key, LimitRequests limitAttribute)
        {
            var clientStat = await _cache.GetCacheValueAsync<ClientStatistics>(key) ?? new ClientStatistics
            {
                LastSuccessfulResponseTime = DateTime.UtcNow,
                NumberOfRequestsCompletedSuccessfully = 0
            };

            int remaining = limitAttribute.MaxRequests - clientStat.NumberOfRequestsCompletedSuccessfully;
            if (remaining < 0) remaining = 0;

            context.Response.Headers["X-RateLimit-Limit"] = limitAttribute.MaxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();

            var resetTime = clientStat.LastSuccessfulResponseTime.AddSeconds(limitAttribute.TimeWindow);
            context.Response.Headers["X-RateLimit-Reset"] = ((long)(resetTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }

        protected string GetClientIpAddress(HttpContext context)
        {
            string ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "unknown";
            }

            return ipAddress;
        }
    }
}
