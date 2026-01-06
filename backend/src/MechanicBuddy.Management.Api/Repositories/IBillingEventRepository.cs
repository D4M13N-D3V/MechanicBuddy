using MechanicBuddy.Management.Api.Domain;

namespace MechanicBuddy.Management.Api.Repositories;

public interface IBillingEventRepository
{
    Task<BillingEvent?> GetByIdAsync(int id);
    Task<IEnumerable<BillingEvent>> GetByTenantIdAsync(string tenantId, int skip = 0, int take = 50);
    Task<IEnumerable<BillingEvent>> GetByEventTypeAsync(string eventType);
    Task<int> CreateAsync(BillingEvent billingEvent);
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
}
