using MechanicBuddy.Management.Api.Domain;

namespace MechanicBuddy.Management.Api.Repositories;

public interface ITenantMetricsRepository
{
    Task<IEnumerable<TenantMetrics>> GetByTenantIdAsync(string tenantId, DateTime? startDate = null, DateTime? endDate = null);
    Task<TenantMetrics?> GetLatestByTenantIdAsync(string tenantId);
    Task<int> CreateAsync(TenantMetrics metrics);
    Task<Dictionary<string, object>> GetAggregateMetricsAsync();
}
