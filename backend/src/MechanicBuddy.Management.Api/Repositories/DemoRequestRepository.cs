using Dapper;
using MechanicBuddy.Management.Api.Domain;
using Npgsql;

namespace MechanicBuddy.Management.Api.Repositories;

public class DemoRequestRepository : IDemoRequestRepository
{
    private readonly string _connectionString;

    public DemoRequestRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Management")
            ?? throw new InvalidOperationException("Management connection string not found");

        // Set up Dapper to use snake_case column mapping
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task<DemoRequest?> GetByIdAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.demo_requests WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<DemoRequest>(sql, new { Id = id });
    }

    public async Task<DemoRequest?> GetByEmailAsync(string email)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.demo_requests WHERE email = @Email ORDER BY created_at DESC LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<DemoRequest>(sql, new { Email = email });
    }

    public async Task<DemoRequest?> GetByTenantIdAsync(string tenantId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.demo_requests WHERE tenant_id = @TenantId";
        return await connection.QuerySingleOrDefaultAsync<DemoRequest>(sql, new { TenantId = tenantId });
    }

    public async Task<IEnumerable<DemoRequest>> GetAllAsync(int skip = 0, int take = 50)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"SELECT * FROM management.demo_requests
                    ORDER BY created_at DESC
                    OFFSET @Skip LIMIT @Take";
        return await connection.QueryAsync<DemoRequest>(sql, new { Skip = skip, Take = take });
    }

    public async Task<IEnumerable<DemoRequest>> GetByStatusAsync(string status)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.demo_requests WHERE status = @Status ORDER BY created_at DESC";
        return await connection.QueryAsync<DemoRequest>(sql, new { Status = status });
    }

    public async Task<int> CreateAsync(DemoRequest demoRequest)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            INSERT INTO management.demo_requests (
                email, company_name, phone_number, message, ip_address, status, tenant_id,
                created_at, approved_at, expires_at, expiring_soon_email_sent_at, notes, rejection_reason
            ) VALUES (
                @Email, @CompanyName, @PhoneNumber, @Message, @IpAddress, @Status, @TenantId,
                @CreatedAt, @ApprovedAt, @ExpiresAt, @ExpiringSoonEmailSentAt, @Notes, @RejectionReason
            ) RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, demoRequest);
    }

    public async Task<bool> UpdateAsync(DemoRequest demoRequest)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            UPDATE management.demo_requests SET
                email = @Email,
                company_name = @CompanyName,
                phone_number = @PhoneNumber,
                message = @Message,
                ip_address = @IpAddress,
                status = @Status,
                tenant_id = @TenantId,
                approved_at = @ApprovedAt,
                expires_at = @ExpiresAt,
                expiring_soon_email_sent_at = @ExpiringSoonEmailSentAt,
                notes = @Notes,
                rejection_reason = @RejectionReason
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, demoRequest);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "DELETE FROM management.demo_requests WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> GetPendingCountAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT COUNT(*) FROM management.demo_requests WHERE status = 'pending'";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<IEnumerable<DemoRequest>> GetExpiringSoonAsync(int daysBeforeExpiration)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            SELECT * FROM management.demo_requests
            WHERE status = 'approved'
              AND expires_at IS NOT NULL
              AND expires_at BETWEEN @Now AND @ExpiryThreshold
              AND expiring_soon_email_sent_at IS NULL
            ORDER BY expires_at";

        var now = DateTime.UtcNow;
        var expiryThreshold = now.AddDays(daysBeforeExpiration);

        return await connection.QueryAsync<DemoRequest>(sql, new { Now = now, ExpiryThreshold = expiryThreshold });
    }

    public async Task<IEnumerable<DemoRequest>> GetExpiredAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            SELECT * FROM management.demo_requests
            WHERE status = 'approved'
              AND expires_at IS NOT NULL
              AND expires_at < @Now
            ORDER BY expires_at";

        return await connection.QueryAsync<DemoRequest>(sql, new { Now = DateTime.UtcNow });
    }

    public async Task<int> GetRequestCountByIpInLastDaysAsync(string ipAddress, int days)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            SELECT COUNT(*) FROM management.demo_requests
            WHERE ip_address = @IpAddress
              AND created_at > @Threshold";

        var threshold = DateTime.UtcNow.AddDays(-days);
        return await connection.ExecuteScalarAsync<int>(sql, new { IpAddress = ipAddress, Threshold = threshold });
    }

    public async Task<int> GetCountByStatusAsync(string status)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT COUNT(*) FROM management.demo_requests WHERE status = @Status";
        return await connection.ExecuteScalarAsync<int>(sql, new { Status = status });
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT COUNT(*) FROM management.demo_requests";
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}
