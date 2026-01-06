using Dapper;
using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Services;
using Npgsql;

namespace MechanicBuddy.Management.Api.Repositories;

public class SuperAdminRepository : ISuperAdminRepository
{
    private readonly string _connectionString;

    public SuperAdminRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Management")
            ?? throw new InvalidOperationException("Management connection string not found");

        // Set up Dapper to use snake_case column mapping
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task<SuperAdmin?> GetByIdAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.super_admins WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<SuperAdmin>(sql, new { Id = id });
    }

    public async Task<SuperAdmin?> GetByEmailAsync(string email)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.super_admins WHERE email = @Email";
        return await connection.QuerySingleOrDefaultAsync<SuperAdmin>(sql, new { Email = email });
    }

    public async Task<IEnumerable<SuperAdmin>> GetAllAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.super_admins ORDER BY created_at DESC";
        return await connection.QueryAsync<SuperAdmin>(sql);
    }

    public async Task<int> CreateAsync(SuperAdmin admin)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            INSERT INTO management.super_admins (
                email, password_hash, name, role, is_active, created_at, last_login_at
            ) VALUES (
                @Email, @PasswordHash, @Name, @Role, @IsActive, @CreatedAt, @LastLoginAt
            ) RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, admin);
    }

    public async Task<bool> UpdateAsync(SuperAdmin admin)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            UPDATE management.super_admins SET
                email = @Email,
                password_hash = @PasswordHash,
                name = @Name,
                role = @Role,
                is_active = @IsActive,
                last_login_at = @LastLoginAt
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, admin);
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "UPDATE management.super_admins SET last_login_at = @Now WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Now = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "DELETE FROM management.super_admins WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task RecordTenantAccessAsync(int adminId, string tenantId, DateTime accessedAt)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            INSERT INTO management.super_admin_access_logs (super_admin_id, tenant_id, accessed_at)
            VALUES (@AdminId, @TenantId, @AccessedAt)";

        await connection.ExecuteAsync(sql, new { AdminId = adminId, TenantId = tenantId, AccessedAt = accessedAt });
    }

    public async Task<IEnumerable<TenantAccessLog>> GetAccessLogsAsync(int? adminId = null, string? tenantId = null, int limit = 100)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            SELECT
                l.id,
                l.super_admin_id,
                a.email as super_admin_email,
                l.tenant_id,
                t.company_name as tenant_name,
                l.accessed_at,
                l.ip_address
            FROM management.super_admin_access_logs l
            JOIN management.super_admins a ON l.super_admin_id = a.id
            LEFT JOIN management.tenants t ON l.tenant_id = t.tenant_id
            WHERE (@AdminId IS NULL OR l.super_admin_id = @AdminId)
              AND (@TenantId IS NULL OR l.tenant_id = @TenantId)
            ORDER BY l.accessed_at DESC
            LIMIT @Limit";

        return await connection.QueryAsync<TenantAccessLog>(sql, new { AdminId = adminId, TenantId = tenantId, Limit = limit });
    }

    public async Task StoreOneTimeTokenAsync(string token, int adminId, string tenantId, DateTime expiresAt)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            INSERT INTO management.super_admin_access_tokens (token, super_admin_id, tenant_id, expires_at, created_at)
            VALUES (@Token, @AdminId, @TenantId, @ExpiresAt, @CreatedAt)";

        await connection.ExecuteAsync(sql, new
        {
            Token = token,
            AdminId = adminId,
            TenantId = tenantId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<(int AdminId, string TenantId)?> ValidateAndConsumeTokenAsync(string token)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        // Get and delete the token in one operation
        var sql = @"
            DELETE FROM management.super_admin_access_tokens
            WHERE token = @Token AND expires_at > @Now AND consumed_at IS NULL
            RETURNING super_admin_id, tenant_id";

        var result = await connection.QuerySingleOrDefaultAsync<(int super_admin_id, string tenant_id)?>(
            sql, new { Token = token, Now = DateTime.UtcNow });

        if (result.HasValue)
        {
            return (result.Value.super_admin_id, result.Value.tenant_id);
        }

        return null;
    }
}
