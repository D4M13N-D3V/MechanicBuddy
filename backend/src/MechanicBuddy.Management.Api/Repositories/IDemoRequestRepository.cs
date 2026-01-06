using MechanicBuddy.Management.Api.Domain;

namespace MechanicBuddy.Management.Api.Repositories;

public interface IDemoRequestRepository
{
    Task<DemoRequest?> GetByIdAsync(int id);
    Task<DemoRequest?> GetByEmailAsync(string email);
    Task<DemoRequest?> GetByTenantIdAsync(string tenantId);
    Task<IEnumerable<DemoRequest>> GetAllAsync(int skip = 0, int take = 50);
    Task<IEnumerable<DemoRequest>> GetByStatusAsync(string status);
    Task<IEnumerable<DemoRequest>> GetExpiringSoonAsync(int daysBeforeExpiration);
    Task<IEnumerable<DemoRequest>> GetExpiredAsync();
    Task<int> GetRequestCountByIpInLastDaysAsync(string ipAddress, int days);
    Task<int> CreateAsync(DemoRequest demoRequest);
    Task<bool> UpdateAsync(DemoRequest demoRequest);
    Task<bool> DeleteAsync(int id);
    Task<int> GetPendingCountAsync();
    Task<int> GetCountByStatusAsync(string status);
    Task<int> GetTotalCountAsync();
}
