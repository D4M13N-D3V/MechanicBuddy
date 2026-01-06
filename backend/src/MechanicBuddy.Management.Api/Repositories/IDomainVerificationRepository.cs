using MechanicBuddy.Management.Api.Domain;

namespace MechanicBuddy.Management.Api.Repositories;

public interface IDomainVerificationRepository
{
    Task<DomainVerification?> GetByIdAsync(int id);
    Task<DomainVerification?> GetByDomainAsync(string domain);
    Task<DomainVerification?> GetByTokenAsync(string token);
    Task<IEnumerable<DomainVerification>> GetByTenantIdAsync(int tenantId);
    Task<int> CreateAsync(DomainVerification verification);
    Task<bool> UpdateAsync(DomainVerification verification);
    Task<bool> DeleteAsync(int id);
}
