using MechanicBuddy.Management.Api.Domain;

namespace MechanicBuddy.Management.Api.Repositories;

public interface ISuperAdminRepository
{
    Task<SuperAdmin?> GetByIdAsync(int id);
    Task<SuperAdmin?> GetByEmailAsync(string email);
    Task<IEnumerable<SuperAdmin>> GetAllAsync();
    Task<int> CreateAsync(SuperAdmin admin);
    Task<bool> UpdateAsync(SuperAdmin admin);
    Task<bool> UpdateLastLoginAsync(int id);
    Task<bool> DeleteAsync(int id);
}
