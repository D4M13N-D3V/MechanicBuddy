using Dapper;
using MechanicBuddy.Management.Api.Domain;
using Npgsql;

namespace MechanicBuddy.Management.Api.Repositories;

public class SuperAdminRepository : ISuperAdminRepository
{
    private readonly string _connectionString;

    public SuperAdminRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Management")
            ?? throw new InvalidOperationException("Management connection string not found");
    }

    public async Task<SuperAdmin?> GetByIdAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM super_admins WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<SuperAdmin>(sql, new { Id = id });
    }

    public async Task<SuperAdmin?> GetByEmailAsync(string email)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM super_admins WHERE email = @Email";
        return await connection.QuerySingleOrDefaultAsync<SuperAdmin>(sql, new { Email = email });
    }

    public async Task<IEnumerable<SuperAdmin>> GetAllAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM super_admins ORDER BY created_at DESC";
        return await connection.QueryAsync<SuperAdmin>(sql);
    }

    public async Task<int> CreateAsync(SuperAdmin admin)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            INSERT INTO super_admins (
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
            UPDATE super_admins SET
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
        var sql = "UPDATE super_admins SET last_login_at = @Now WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Now = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "DELETE FROM super_admins WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}
