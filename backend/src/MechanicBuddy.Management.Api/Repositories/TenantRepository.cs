using Dapper;
using MechanicBuddy.Management.Api.Domain;
using Npgsql;
using System.Text.Json;

namespace MechanicBuddy.Management.Api.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly string _connectionString;

    public TenantRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Management")
            ?? throw new InvalidOperationException("Management connection string not found");
    }

    public async Task<Tenant?> GetByIdAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.tenants WHERE id = @Id";
        var tenant = await connection.QuerySingleOrDefaultAsync<Tenant>(sql, new { Id = id });
        return tenant;
    }

    public async Task<Tenant?> GetByTenantIdAsync(string tenantId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.tenants WHERE tenant_id = @TenantId";
        return await connection.QuerySingleOrDefaultAsync<Tenant>(sql, new { TenantId = tenantId });
    }

    public async Task<Tenant?> GetByEmailAsync(string email)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.tenants WHERE owner_email = @Email";
        return await connection.QuerySingleOrDefaultAsync<Tenant>(sql, new { Email = email });
    }

    public async Task<Tenant?> GetByCustomDomainAsync(string domain)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.tenants WHERE custom_domain = @Domain";
        return await connection.QuerySingleOrDefaultAsync<Tenant>(sql, new { Domain = domain });
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(int skip = 0, int take = 50)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"SELECT * FROM management.tenants
                    ORDER BY created_at DESC
                    OFFSET @Skip LIMIT @Take";
        return await connection.QueryAsync<Tenant>(sql, new { Skip = skip, Take = take });
    }

    public async Task<IEnumerable<Tenant>> GetByStatusAsync(string status)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.tenants WHERE status = @Status ORDER BY created_at DESC";
        return await connection.QueryAsync<Tenant>(sql, new { Status = status });
    }

    public async Task<IEnumerable<Tenant>> GetByOwnerEmailAsync(string ownerEmail)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT * FROM management.tenants WHERE owner_email = @OwnerEmail ORDER BY created_at DESC";
        return await connection.QueryAsync<Tenant>(sql, new { OwnerEmail = ownerEmail });
    }

    public async Task<int> CreateAsync(Tenant tenant)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            INSERT INTO management.tenants (
                tenant_id, company_name, tier, status, owner_email, owner_name,
                stripe_customer_id, stripe_subscription_id, custom_domain, domain_verified,
                created_at, trial_ends_at, subscription_ends_at, max_mechanics, max_storage,
                is_demo, k8s_namespace, db_connection_string, api_url, metadata
            ) VALUES (
                @TenantId, @CompanyName, @Tier, @Status, @OwnerEmail, @OwnerName,
                @StripeCustomerId, @StripeSubscriptionId, @CustomDomain, @DomainVerified,
                @CreatedAt, @TrialEndsAt, @SubscriptionEndsAt, @MaxMechanics, @MaxStorage,
                @IsDemo, @K8sNamespace, @DbConnectionString, @ApiUrl, @Metadata::jsonb
            ) RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            tenant.TenantId,
            tenant.CompanyName,
            tenant.Tier,
            tenant.Status,
            tenant.OwnerEmail,
            tenant.OwnerName,
            tenant.StripeCustomerId,
            tenant.StripeSubscriptionId,
            tenant.CustomDomain,
            tenant.DomainVerified,
            tenant.CreatedAt,
            tenant.TrialEndsAt,
            tenant.SubscriptionEndsAt,
            tenant.MaxMechanics,
            tenant.MaxStorage,
            tenant.IsDemo,
            tenant.K8sNamespace,
            tenant.DbConnectionString,
            tenant.ApiUrl,
            Metadata = tenant.Metadata != null ? JsonSerializer.Serialize(tenant.Metadata) : null
        });
    }

    public async Task<bool> UpdateAsync(Tenant tenant)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            UPDATE management.tenants SET
                company_name = @CompanyName,
                tier = @Tier,
                status = @Status,
                owner_email = @OwnerEmail,
                owner_name = @OwnerName,
                stripe_customer_id = @StripeCustomerId,
                stripe_subscription_id = @StripeSubscriptionId,
                custom_domain = @CustomDomain,
                domain_verified = @DomainVerified,
                trial_ends_at = @TrialEndsAt,
                subscription_ends_at = @SubscriptionEndsAt,
                max_mechanics = @MaxMechanics,
                max_storage = @MaxStorage,
                is_demo = @IsDemo,
                k8s_namespace = @K8sNamespace,
                db_connection_string = @DbConnectionString,
                api_url = @ApiUrl,
                metadata = @Metadata::jsonb
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            tenant.Id,
            tenant.CompanyName,
            tenant.Tier,
            tenant.Status,
            tenant.OwnerEmail,
            tenant.OwnerName,
            tenant.StripeCustomerId,
            tenant.StripeSubscriptionId,
            tenant.CustomDomain,
            tenant.DomainVerified,
            tenant.TrialEndsAt,
            tenant.SubscriptionEndsAt,
            tenant.MaxMechanics,
            tenant.MaxStorage,
            tenant.IsDemo,
            tenant.K8sNamespace,
            tenant.DbConnectionString,
            tenant.ApiUrl,
            Metadata = tenant.Metadata != null ? JsonSerializer.Serialize(tenant.Metadata) : null
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "DELETE FROM management.tenants WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT COUNT(*) FROM management.tenants";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<Dictionary<string, int>> GetCountByTierAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT tier, COUNT(*)::int as count FROM management.tenants GROUP BY tier";
        var results = await connection.QueryAsync<(string tier, int count)>(sql);
        return results.ToDictionary(r => r.tier, r => r.count);
    }

    public async Task<Dictionary<string, int>> GetCountByStatusAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT status, COUNT(*)::int as count FROM management.tenants GROUP BY status";
        var results = await connection.QueryAsync<(string status, int count)>(sql);
        return results.ToDictionary(r => r.status, r => r.count);
    }

    public async Task<int> GetCountCreatedBetweenAsync(DateTime start, DateTime end)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = "SELECT COUNT(*) FROM management.tenants WHERE created_at >= @Start AND created_at < @End";
        return await connection.ExecuteScalarAsync<int>(sql, new { Start = start, End = end });
    }

    public async Task<IEnumerable<Tenant>> GetExpiredSubscriptionsAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        // Get tenants where:
        // - subscription_ends_at has passed
        // - no active subscription (stripe_subscription_id is null)
        // - tier is not already solo/free/lifetime
        var sql = @"SELECT * FROM management.tenants
                    WHERE subscription_ends_at IS NOT NULL
                    AND subscription_ends_at < @Now
                    AND stripe_subscription_id IS NULL
                    AND tier NOT IN ('solo', 'free', 'lifetime')";
        return await connection.QueryAsync<Tenant>(sql, new { Now = DateTime.UtcNow });
    }

    public async Task<int> GetTotalSubscriptionMonthsByTierAsync(string tier)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        // Calculate total months subscribed for all tenants of a given tier
        // Each tenant contributes: ceil(months since created_at)
        // Minimum 1 month per tenant
        var sql = @"
            SELECT COALESCE(SUM(
                GREATEST(1, CEIL(EXTRACT(EPOCH FROM (NOW() - created_at)) / (30 * 24 * 60 * 60)))
            ), 0)::int
            FROM management.tenants
            WHERE tier = @Tier";
        return await connection.ExecuteScalarAsync<int>(sql, new { Tier = tier });
    }
}
