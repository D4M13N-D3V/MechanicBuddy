using Dapper;
using MechanicBuddy.Management.Api.Domain;
using Npgsql;

namespace MechanicBuddy.Management.Api.Repositories;

public class TenantMetricsRepository : ITenantMetricsRepository
{
    private readonly string _connectionString;

    public TenantMetricsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Management")
            ?? throw new InvalidOperationException("Management connection string not found");
    }

    public async Task<IEnumerable<TenantMetrics>> GetByTenantIdAsync(string tenantId, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"SELECT * FROM tenant_metrics
                    WHERE tenant_id = @TenantId
                    AND (@StartDate IS NULL OR recorded_at >= @StartDate)
                    AND (@EndDate IS NULL OR recorded_at <= @EndDate)
                    ORDER BY recorded_at DESC";

        return await connection.QueryAsync<TenantMetrics>(sql, new
        {
            TenantId = tenantId,
            StartDate = startDate,
            EndDate = endDate
        });
    }

    public async Task<TenantMetrics?> GetLatestByTenantIdAsync(string tenantId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"SELECT * FROM tenant_metrics
                    WHERE tenant_id = @TenantId
                    ORDER BY recorded_at DESC
                    LIMIT 1";

        return await connection.QuerySingleOrDefaultAsync<TenantMetrics>(sql, new { TenantId = tenantId });
    }

    public async Task<int> CreateAsync(TenantMetrics metrics)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            INSERT INTO tenant_metrics (
                tenant_id, active_mechanics, work_orders_count, clients_count,
                vehicles_count, storage_used, api_calls_count, recorded_at
            ) VALUES (
                @TenantId, @ActiveMechanics, @WorkOrdersCount, @ClientsCount,
                @VehiclesCount, @StorageUsed, @ApiCallsCount, @RecordedAt
            ) RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, metrics);
    }

    public async Task<Dictionary<string, object>> GetAggregateMetricsAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            WITH latest_metrics AS (
                SELECT DISTINCT ON (tenant_id)
                    tenant_id,
                    active_mechanics,
                    work_orders_count,
                    clients_count,
                    vehicles_count,
                    storage_used,
                    api_calls_count
                FROM tenant_metrics
                ORDER BY tenant_id, recorded_at DESC
            )
            SELECT
                COUNT(DISTINCT tenant_id)::int as total_tenants,
                COALESCE(SUM(active_mechanics), 0)::int as total_mechanics,
                COALESCE(SUM(work_orders_count), 0)::int as total_work_orders,
                COALESCE(SUM(clients_count), 0)::int as total_clients,
                COALESCE(SUM(vehicles_count), 0)::int as total_vehicles,
                COALESCE(SUM(storage_used), 0)::bigint as total_storage_used,
                COALESCE(SUM(api_calls_count), 0)::int as total_api_calls
            FROM latest_metrics";

        var result = await connection.QuerySingleAsync(sql);

        return new Dictionary<string, object>
        {
            ["totalTenants"] = result.total_tenants,
            ["totalMechanics"] = result.total_mechanics,
            ["totalWorkOrders"] = result.total_work_orders,
            ["totalClients"] = result.total_clients,
            ["totalVehicles"] = result.total_vehicles,
            ["totalStorageUsed"] = result.total_storage_used,
            ["totalApiCalls"] = result.total_api_calls
        };
    }
}
