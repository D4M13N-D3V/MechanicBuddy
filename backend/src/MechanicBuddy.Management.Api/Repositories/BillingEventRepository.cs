using Dapper;
using MechanicBuddy.Management.Api.Domain;
using Npgsql;
using System.Text.Json;

namespace MechanicBuddy.Management.Api.Repositories;

public class BillingEventRepository : IBillingEventRepository
{
    private readonly string _connectionString;

    public BillingEventRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Management")
            ?? throw new InvalidOperationException("Management connection string not found");
    }

    public async Task<BillingEvent?> GetByIdAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM billing_events WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<BillingEvent>(sql, new { Id = id });
    }

    public async Task<IEnumerable<BillingEvent>> GetByTenantIdAsync(string tenantId, int skip = 0, int take = 50)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"SELECT * FROM billing_events
                    WHERE tenant_id = @TenantId
                    ORDER BY created_at DESC
                    OFFSET @Skip LIMIT @Take";

        return await connection.QueryAsync<BillingEvent>(sql, new
        {
            TenantId = tenantId,
            Skip = skip,
            Take = take
        });
    }

    public async Task<IEnumerable<BillingEvent>> GetByEventTypeAsync(string eventType)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"SELECT * FROM billing_events
                    WHERE event_type = @EventType
                    ORDER BY created_at DESC";

        return await connection.QueryAsync<BillingEvent>(sql, new { EventType = eventType });
    }

    public async Task<int> CreateAsync(BillingEvent billingEvent)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            INSERT INTO billing_events (
                tenant_id, event_type, amount, currency, stripe_event_id,
                invoice_id, created_at, metadata
            ) VALUES (
                @TenantId, @EventType, @Amount, @Currency, @StripeEventId,
                @InvoiceId, @CreatedAt, @Metadata::jsonb
            ) RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            billingEvent.TenantId,
            billingEvent.EventType,
            billingEvent.Amount,
            billingEvent.Currency,
            billingEvent.StripeEventId,
            billingEvent.InvoiceId,
            billingEvent.CreatedAt,
            Metadata = billingEvent.Metadata != null ? JsonSerializer.Serialize(billingEvent.Metadata) : null
        });
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"SELECT COALESCE(SUM(amount), 0) FROM billing_events
                    WHERE event_type = 'payment_succeeded'
                    AND (@StartDate IS NULL OR created_at >= @StartDate)
                    AND (@EndDate IS NULL OR created_at <= @EndDate)";

        return await connection.ExecuteScalarAsync<decimal>(sql, new
        {
            StartDate = startDate,
            EndDate = endDate
        });
    }

    public async Task<IEnumerable<BillingEvent>> GetAllAsync(int skip = 0, int take = 50)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"SELECT * FROM billing_events
                    ORDER BY created_at DESC
                    OFFSET @Skip LIMIT @Take";

        return await connection.QueryAsync<BillingEvent>(sql, new { Skip = skip, Take = take });
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT COUNT(*) FROM billing_events";
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}
