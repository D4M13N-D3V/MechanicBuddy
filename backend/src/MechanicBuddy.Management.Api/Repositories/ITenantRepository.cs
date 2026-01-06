using MechanicBuddy.Management.Api.Domain;

namespace MechanicBuddy.Management.Api.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(int id);
    Task<Tenant?> GetByTenantIdAsync(string tenantId);
    Task<Tenant?> GetByEmailAsync(string email);
    Task<Tenant?> GetByCustomDomainAsync(string domain);
    Task<IEnumerable<Tenant>> GetAllAsync(int skip = 0, int take = 50);
    Task<IEnumerable<Tenant>> GetByStatusAsync(string status);
    Task<int> CreateAsync(Tenant tenant);
    Task<bool> UpdateAsync(Tenant tenant);
    Task<bool> DeleteAsync(int id);
    Task<int> GetTotalCountAsync();
    Task<Dictionary<string, int>> GetCountByTierAsync();
    Task<Dictionary<string, int>> GetCountByStatusAsync();
    Task<int> GetCountCreatedBetweenAsync(DateTime start, DateTime end);
}
