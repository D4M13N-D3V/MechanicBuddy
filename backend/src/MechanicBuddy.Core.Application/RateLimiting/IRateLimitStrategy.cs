using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Carmasters.Core.Application.RateLimiting
{
    public interface IRateLimitStrategy
    {
        string GenerateRateLimitKey(HttpContext context, LimitRequests limitAttribute);
        Task<bool> IsRateLimitExceeded(string key, LimitRequests limitAttribute);
        Task UpdateRateLimitStatistics(string key, LimitRequests limitAttribute);
        Task HandleRateLimitExceeded(HttpContext context, LimitRequests limitAttribute, string key);
        Task AddRateLimitHeaders(HttpContext context, string key, LimitRequests limitAttribute);
    }
}
