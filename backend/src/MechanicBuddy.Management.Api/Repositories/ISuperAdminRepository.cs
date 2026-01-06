using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Services;

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

    // Tenant access audit logging
    Task RecordTenantAccessAsync(int adminId, string tenantId, DateTime accessedAt);
    Task<IEnumerable<TenantAccessLog>> GetAccessLogsAsync(int? adminId = null, string? tenantId = null, int limit = 100);

    // One-time access tokens
    Task StoreOneTimeTokenAsync(string token, int adminId, string tenantId, DateTime expiresAt);
    Task<(int AdminId, string TenantId)?> ValidateAndConsumeTokenAsync(string token);
}
