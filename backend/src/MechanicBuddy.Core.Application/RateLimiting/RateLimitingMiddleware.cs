using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Caching.Distributed;

namespace Carmasters.Core.Application.RateLimiting
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        private readonly RateLimitStrategyFactory _strategyFactory;

        public RateLimitingMiddleware(
            RequestDelegate next,
            IDistributedCache cache)
        {
            _next = next;
            _cache = cache;
            _strategyFactory = new RateLimitStrategyFactory(cache);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var limitAttribute = endpoint?.Metadata.GetMetadata<LimitRequests>();

            if (limitAttribute is null)
            {
                await _next(context);
                return;
            }

            var strategy = _strategyFactory.CreateStrategy(context, limitAttribute);
            var key = strategy.GenerateRateLimitKey(context, limitAttribute);

            if (await strategy.IsRateLimitExceeded(key, limitAttribute))
            {
                await strategy.HandleRateLimitExceeded(context, limitAttribute, key);
                return;
            }

            // Add headers before processing the request
            await strategy.AddRateLimitHeaders(context, key, limitAttribute);

            // Process the request
            await _next(context);

            // Update statistics after processing
            await strategy.UpdateRateLimitStatistics(key, limitAttribute);
        }
    }
}
