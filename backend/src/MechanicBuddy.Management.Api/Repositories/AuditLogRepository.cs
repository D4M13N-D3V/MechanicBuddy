using Dapper;
using MechanicBuddy.Management.Api.Domain;
using Npgsql;

namespace MechanicBuddy.Management.Api.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly string _connectionString;

    public AuditLogRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Management")
            ?? throw new InvalidOperationException("Management connection string not found");

        // Set up Dapper to use snake_case column mapping
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task<int> CreateAsync(AuditLog log)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            INSERT INTO management.audit_logs (
                admin_id, admin_email, admin_role, ip_address, user_agent,
                action_type, http_method, endpoint, resource_type, resource_id,
                tenant_id, action_description, timestamp, duration_ms,
                status_code, was_successful
            ) VALUES (
                @AdminId, @AdminEmail, @AdminRole, @IpAddress, @UserAgent,
                @ActionType, @HttpMethod, @Endpoint, @ResourceType, @ResourceId,
                @TenantId, @ActionDescription, @Timestamp, @DurationMs,
                @StatusCode, @WasSuccessful
            ) RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, log);
    }

    public async Task<IEnumerable<AuditLog>> GetPageAsync(
        string? searchText = null,
        string? actionType = null,
        string? tenantId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int limit = 50,
        int offset = 0)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var (whereClause, parameters) = BuildWhereClause(searchText, actionType, tenantId, fromDate, toDate);
        parameters.Add("Limit", limit);
        parameters.Add("Offset", offset);

        var sql = $@"
            SELECT * FROM management.audit_logs
            {whereClause}
            ORDER BY timestamp DESC
            LIMIT @Limit OFFSET @Offset";

        return await connection.QueryAsync<AuditLog>(sql, parameters);
    }

    public async Task<int> GetCountAsync(
        string? searchText = null,
        string? actionType = null,
        string? tenantId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var (whereClause, parameters) = BuildWhereClause(searchText, actionType, tenantId, fromDate, toDate);
        var sql = $"SELECT COUNT(*) FROM management.audit_logs {whereClause}";
        return await connection.QuerySingleAsync<int>(sql, parameters);
    }

    public async Task<AuditLogStats> GetStatsAsync(int days = 7)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var fromDate = DateTime.UtcNow.AddDays(-days);

        var stats = await connection.QuerySingleAsync<AuditLogStats>(@"
            SELECT
                COUNT(*) as TotalRequests,
                COUNT(DISTINCT admin_email) as UniqueAdmins,
                COUNT(*) FILTER (WHERE action_type = 'tenant_operation') as TenantOperations,
                COUNT(*) FILTER (WHERE action_type = 'auth') as AuthEvents,
                COUNT(*) FILTER (WHERE was_successful = false) as FailedRequests
            FROM management.audit_logs
            WHERE timestamp >= @FromDate",
            new { FromDate = fromDate });

        return stats;
    }

    private (string whereClause, DynamicParameters parameters) BuildWhereClause(
        string? searchText, string? actionType, string? tenantId, DateTime? fromDate, DateTime? toDate)
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(searchText))
        {
            whereClause += @" AND (LOWER(admin_email) LIKE @SearchText
                                OR LOWER(endpoint) LIKE @SearchText
                                OR LOWER(action_description) LIKE @SearchText)";
            parameters.Add("SearchText", $"%{searchText.ToLower()}%");
        }

        if (!string.IsNullOrEmpty(actionType))
        {
            whereClause += " AND action_type = @ActionType";
            parameters.Add("ActionType", actionType);
        }

        if (!string.IsNullOrEmpty(tenantId))
        {
            whereClause += " AND tenant_id = @TenantId";
            parameters.Add("TenantId", tenantId);
        }

        if (fromDate.HasValue)
        {
            whereClause += " AND timestamp >= @FromDate";
            parameters.Add("FromDate", fromDate.Value);
        }

        if (toDate.HasValue)
        {
            whereClause += " AND timestamp <= @ToDate";
            parameters.Add("ToDate", toDate.Value);
        }

        return (whereClause, parameters);
    }
}
