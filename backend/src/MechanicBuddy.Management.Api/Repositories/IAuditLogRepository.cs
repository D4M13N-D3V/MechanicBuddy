using MechanicBuddy.Management.Api.Domain;

namespace MechanicBuddy.Management.Api.Repositories;

public interface IAuditLogRepository
{
    Task<int> CreateAsync(AuditLog log);

    Task<IEnumerable<AuditLog>> GetPageAsync(
        string? searchText = null,
        string? actionType = null,
        string? tenantId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int limit = 50,
        int offset = 0);

    Task<int> GetCountAsync(
        string? searchText = null,
        string? actionType = null,
        string? tenantId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    Task<AuditLogStats> GetStatsAsync(int days = 7);
}
