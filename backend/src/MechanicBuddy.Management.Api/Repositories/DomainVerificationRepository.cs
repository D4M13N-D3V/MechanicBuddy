using Dapper;
using MechanicBuddy.Management.Api.Domain;
using Npgsql;

namespace MechanicBuddy.Management.Api.Repositories;

public class DomainVerificationRepository : IDomainVerificationRepository
{
    private readonly string _connectionString;

    public DomainVerificationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Management")
            ?? throw new InvalidOperationException("Management connection string not found");
    }

    public async Task<DomainVerification?> GetByIdAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.domain_verifications WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<DomainVerification>(sql, new { Id = id });
    }

    public async Task<DomainVerification?> GetByDomainAsync(string domain)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.domain_verifications WHERE domain = @Domain ORDER BY created_at DESC LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<DomainVerification>(sql, new { Domain = domain });
    }

    public async Task<DomainVerification?> GetByTokenAsync(string token)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.domain_verifications WHERE verification_token = @Token";
        return await connection.QuerySingleOrDefaultAsync<DomainVerification>(sql, new { Token = token });
    }

    public async Task<IEnumerable<DomainVerification>> GetByTenantIdAsync(int tenantId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.domain_verifications WHERE tenant_id = @TenantId ORDER BY created_at DESC";
        return await connection.QueryAsync<DomainVerification>(sql, new { TenantId = tenantId });
    }

    public async Task<int> CreateAsync(DomainVerification verification)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            INSERT INTO management.domain_verifications (
                tenant_id, domain, verification_token, verification_method,
                is_verified, created_at, verified_at, expires_at
            ) VALUES (
                @TenantId, @Domain, @VerificationToken, @VerificationMethod,
                @IsVerified, @CreatedAt, @VerifiedAt, @ExpiresAt
            ) RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, verification);
    }

    public async Task<bool> UpdateAsync(DomainVerification verification)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            UPDATE management.domain_verifications SET
                is_verified = @IsVerified,
                verified_at = @VerifiedAt,
                expires_at = @ExpiresAt
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, verification);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "DELETE FROM management.domain_verifications WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}
