using Npgsql;

namespace MechanicBuddy.Management.Api.Infrastructure;

public class TenantDatabaseProvisioner : ITenantDatabaseProvisioner
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantDatabaseProvisioner> _logger;
    private readonly string _postgresHost;
    private readonly int _postgresPort;
    private readonly string _postgresUser;
    private readonly string _postgresPassword;
    private readonly string _tenantDbPrefix;
    private readonly string _templateDbName;
    private readonly string _tenancyDbName;

    // Template employee ID (from mechanicbuddy-testt template database)
    private const string TemplateEmployeeId = "a7227c1f-3bbc-4367-a9b8-baa83d0f19ca";

    // Default admin password - same as the application default "carcare"
    private const string DefaultAdminPassword = "carcare";
    // BCrypt hash of "carcare" - precomputed for consistency
    private const string DefaultAdminPasswordHash = "$2a$11$zsTS62pGn5Cfca4CgqRJxebx45je/3nJj.puxIArFwtAjHew67m6i";

    public TenantDatabaseProvisioner(
        IConfiguration configuration,
        ILogger<TenantDatabaseProvisioner> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _postgresHost = configuration["Database:PostgresHost"] ?? "postgres";
        _postgresPort = int.TryParse(configuration["Database:PostgresPort"], out var port) ? port : 5432;
        _postgresUser = configuration["Database:PostgresUser"] ?? "postgres";
        _postgresPassword = configuration["Database:PostgresPassword"] ?? "postgres";
        // Tenant databases are named: mechanicbuddy-{tenantId}
        // Use dedicated config key to avoid confusion with management database
        _tenantDbPrefix = configuration["Database:TenantDatabasePrefix"] ?? "mechanicbuddy";
        _templateDbName = configuration["Database:TemplateDatabase"] ?? "mechanicbuddy-testt";
        _tenancyDbName = configuration["Database:TenancyDatabase"] ?? "mechanicbuddy-tenancy";
    }

    public async Task<string> ProvisionTenantDatabaseAsync(string tenantId)
    {
        // Use default PostgreSQL host with default owner info
        return await ProvisionTenantDatabaseAsync(tenantId, null, null, null, null);
    }

    /// <summary>
    /// Provisions a tenant database on a specific PostgreSQL host.
    /// Used for shared free-tier instances where databases are created on a shared cluster.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="targetPostgresHost">Target PostgreSQL host (null = use default).</param>
    /// <param name="targetPostgresPort">Target PostgreSQL port (null = use default).</param>
    /// <param name="ownerEmail">Owner's email address for the admin account (null = use default).</param>
    /// <param name="ownerName">Owner's name for the admin account (null = use default).</param>
    /// <returns>Connection string to the tenant database.</returns>
    public async Task<string> ProvisionTenantDatabaseAsync(string tenantId, string? targetPostgresHost, int? targetPostgresPort, string? ownerEmail = null, string? ownerName = null)
    {
        var tenantDbName = GetTenantDbName(tenantId);
        var postgresHost = targetPostgresHost ?? _postgresHost;
        var postgresPort = targetPostgresPort ?? _postgresPort;

        _logger.LogInformation("Provisioning database {DbName} for tenant {TenantId} from template {Template} on host {Host}:{Port}",
            tenantDbName, tenantId, _templateDbName, postgresHost, postgresPort);

        // Connect to postgres database to create new database
        var adminConnectionString = BuildConnectionString("postgres", postgresHost, postgresPort);
        await using (var adminConnection = new NpgsqlConnection(adminConnectionString))
        {
            await adminConnection.OpenAsync();

            // Check if database already exists
            await using (var checkCmd = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = @dbName", adminConnection))
            {
                checkCmd.Parameters.AddWithValue("dbName", tenantDbName);
                var exists = await checkCmd.ExecuteScalarAsync() != null;

                if (exists)
                {
                    _logger.LogWarning("Database {DbName} already exists for tenant {TenantId}", tenantDbName, tenantId);
                }
                else
                {
                    // Create database from template
                    // Note: Need to use dynamic SQL since parameterized identifiers aren't supported
                    var createDbSql = $"CREATE DATABASE \"{tenantDbName}\" WITH TEMPLATE \"{_templateDbName}\" OWNER \"{_postgresUser}\"";
                    await using var createCmd = new NpgsqlCommand(createDbSql, adminConnection);
                    await createCmd.ExecuteNonQueryAsync();

                    _logger.LogInformation("Created database {DbName} from template {Template}", tenantDbName, _templateDbName);
                }
            }
        }

        // Update the user table in the cloned tenant database to use the correct tenant name
        await UpdateTenantUserTableAsync(tenantId, tenantDbName, postgresHost, postgresPort);

        // Create admin user in tenancy database (always uses default host - shared tenancy DB)
        await CreateDefaultAdminAsync(tenantId, ownerEmail, ownerName);

        _logger.LogInformation("Successfully provisioned database for tenant {TenantId} on host {Host}", tenantId, postgresHost);

        return BuildTenantConnectionString(tenantDbName, postgresHost, postgresPort);
    }

    public async Task DeleteTenantDatabaseAsync(string tenantId)
    {
        // Use default PostgreSQL host
        await DeleteTenantDatabaseAsync(tenantId, null, null);
    }

    /// <summary>
    /// Deletes a tenant database from a specific PostgreSQL host.
    /// Used for shared free-tier instances where databases are hosted on a shared cluster.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="targetPostgresHost">Target PostgreSQL host (null = use default).</param>
    /// <param name="targetPostgresPort">Target PostgreSQL port (null = use default).</param>
    public async Task DeleteTenantDatabaseAsync(string tenantId, string? targetPostgresHost, int? targetPostgresPort)
    {
        var tenantDbName = GetTenantDbName(tenantId);
        var postgresHost = targetPostgresHost ?? _postgresHost;
        var postgresPort = targetPostgresPort ?? _postgresPort;

        _logger.LogInformation("Deleting database {DbName} for tenant {TenantId} from host {Host}:{Port}",
            tenantDbName, tenantId, postgresHost, postgresPort);

        // First, delete the user from tenancy database (always uses default host - shared tenancy DB)
        var tenancyConnectionString = BuildConnectionString(_tenancyDbName);
        await using (var tenancyConnection = new NpgsqlConnection(tenancyConnectionString))
        {
            await tenancyConnection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "DELETE FROM public.\"user\" WHERE tenantname = @tenantId", tenancyConnection);
            cmd.Parameters.AddWithValue("tenantId", tenantId);
            await cmd.ExecuteNonQueryAsync();
        }

        // Then drop the database from the target host (connect to postgres database to do this)
        var adminConnectionString = BuildConnectionString("postgres", postgresHost, postgresPort);
        await using (var adminConnection = new NpgsqlConnection(adminConnectionString))
        {
            await adminConnection.OpenAsync();

            // Terminate any existing connections to the database
            await using (var terminateCmd = new NpgsqlCommand(
                $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @dbName AND pid <> pg_backend_pid()", adminConnection))
            {
                terminateCmd.Parameters.AddWithValue("dbName", tenantDbName);
                await terminateCmd.ExecuteNonQueryAsync();
            }

            // Drop the database
            await using (var dropCmd = new NpgsqlCommand(
                $"DROP DATABASE IF EXISTS \"{tenantDbName}\"", adminConnection))
            {
                await dropCmd.ExecuteNonQueryAsync();
            }
        }

        _logger.LogInformation("Deleted database {DbName} for tenant {TenantId} from host {Host}", tenantDbName, tenantId, postgresHost);
    }

    public async Task<bool> TenantDatabaseExistsAsync(string tenantId)
    {
        // Use default PostgreSQL host
        return await TenantDatabaseExistsAsync(tenantId, null, null);
    }

    /// <summary>
    /// Checks if a tenant database exists on a specific PostgreSQL host.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="targetPostgresHost">Target PostgreSQL host (null = use default).</param>
    /// <param name="targetPostgresPort">Target PostgreSQL port (null = use default).</param>
    /// <returns>True if the database exists.</returns>
    public async Task<bool> TenantDatabaseExistsAsync(string tenantId, string? targetPostgresHost, int? targetPostgresPort)
    {
        var tenantDbName = GetTenantDbName(tenantId);
        var postgresHost = targetPostgresHost ?? _postgresHost;
        var postgresPort = targetPostgresPort ?? _postgresPort;
        var adminConnectionString = BuildConnectionString("postgres", postgresHost, postgresPort);

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @dbName)", connection);
        cmd.Parameters.AddWithValue("dbName", tenantDbName);

        var result = await cmd.ExecuteScalarAsync();
        return result is bool exists && exists;
    }

    public async Task<int> DisableNonAdminUsersAsync(string tenantId)
    {
        // Disable all non-admin users by setting validated = false
        // This is used when downgrading from team tier to solo (only 1 user allowed)
        var tenancyConnectionString = BuildConnectionString(_tenancyDbName);

        await using var connection = new NpgsqlConnection(tenancyConnectionString);
        await connection.OpenAsync();

        // Set validated = false for all users except the default admin (username = 'admin')
        await using var cmd = new NpgsqlCommand(@"
            UPDATE public.""user""
            SET validated = false
            WHERE tenantname = @tenantId
              AND username != 'admin'", connection);
        cmd.Parameters.AddWithValue("tenantId", tenantId);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();

        _logger.LogInformation("Disabled {Count} non-admin users for tenant {TenantId}", rowsAffected, tenantId);

        return rowsAffected;
    }

    private string GetTenantDbName(string tenantId)
    {
        // Database naming: mechanicbuddy-{tenantId}
        return $"{_tenantDbPrefix}-{tenantId}";
    }

    private async Task UpdateTenantUserTableAsync(string tenantId, string tenantDbName, string postgresHost, int postgresPort)
    {
        // Update the user table in the cloned tenant database to use the correct tenant name
        // The template database has users with the template's tenant name, we need to update them
        var tenantConnectionString = BuildConnectionString(tenantDbName, postgresHost, postgresPort);

        await using var connection = new NpgsqlConnection(tenantConnectionString);
        await connection.OpenAsync();

        // Update all users in this database to have the correct tenant name
        await using var cmd = new NpgsqlCommand(
            "UPDATE public.user SET tenantname = @tenantId", connection);
        cmd.Parameters.AddWithValue("tenantId", tenantId);
        var rowsAffected = await cmd.ExecuteNonQueryAsync();

        _logger.LogInformation("Updated {RowCount} user(s) in tenant database {DbName} with tenant name {TenantId}",
            rowsAffected, tenantDbName, tenantId);
    }

    private async Task CreateDefaultAdminAsync(string tenantId, string? ownerEmail = null, string? ownerName = null)
    {
        // Create admin user in the tenancy database (where all users are stored)
        // The employee record already exists in the cloned tenant database from the template
        var tenancyConnectionString = BuildConnectionString(_tenancyDbName);
        var employeeId = Guid.Parse(TemplateEmployeeId);

        // Use provided email or default
        var email = !string.IsNullOrWhiteSpace(ownerEmail) ? ownerEmail : "admin@example.com";

        await using var connection = new NpgsqlConnection(tenancyConnectionString);
        await connection.OpenAsync();

        // Check if admin user already exists for this tenant
        await using (var checkCmd = new NpgsqlCommand(
            @"SELECT COUNT(*) FROM public.""user"" WHERE username = 'admin' AND tenantname = @tenantId", connection))
        {
            checkCmd.Parameters.AddWithValue("tenantId", tenantId);
            var exists = (long)(await checkCmd.ExecuteScalarAsync() ?? 0) > 0;

            if (exists)
            {
                _logger.LogDebug("Admin user already exists for tenant {TenantId}", tenantId);
                return;
            }
        }

        // Create the admin user with the default "carcare" password
        // Users will be prompted to change password on first login
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO public.""user"" (username, password, tenantname, email, validated, profile_image, employeeid, must_change_password)
            VALUES (@username, @password, @tenantname, @email, @validated, @profileImage, @employeeId, @mustChangePassword)", connection))
        {
            cmd.Parameters.AddWithValue("username", "admin");
            cmd.Parameters.AddWithValue("password", DefaultAdminPasswordHash);
            cmd.Parameters.AddWithValue("tenantname", tenantId);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("validated", true);
            cmd.Parameters.AddWithValue("profileImage", DBNull.Value);
            cmd.Parameters.AddWithValue("employeeId", employeeId);
            cmd.Parameters.AddWithValue("mustChangePassword", true); // Force password change on first login
            await cmd.ExecuteNonQueryAsync();
        }

        // Also update the employee name in the tenant database if owner name is provided
        if (!string.IsNullOrWhiteSpace(ownerName))
        {
            await UpdateEmployeeNameAsync(tenantId, employeeId, ownerName);
        }

        _logger.LogInformation("Created default admin user for tenant {TenantId} with email {Email}. Default password: {Password}",
            tenantId, email, DefaultAdminPassword);
    }

    private async Task UpdateEmployeeNameAsync(string tenantId, Guid employeeId, string fullName)
    {
        try
        {
            // Parse the full name into first and last name
            var nameParts = fullName.Trim().Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            var tenantDbName = GetTenantDbName(tenantId);
            var tenantConnectionString = BuildConnectionString(tenantDbName);

            await using var connection = new NpgsqlConnection(tenantConnectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                @"UPDATE domain.employee SET firstname = @firstName, lastname = @lastName WHERE id = @employeeId", connection);
            cmd.Parameters.AddWithValue("firstName", firstName);
            cmd.Parameters.AddWithValue("lastName", lastName);
            cmd.Parameters.AddWithValue("employeeId", employeeId);
            await cmd.ExecuteNonQueryAsync();

            _logger.LogInformation("Updated employee name for tenant {TenantId}: {FirstName} {LastName}", tenantId, firstName, lastName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update employee name for tenant {TenantId}", tenantId);
            // Non-critical - don't fail provisioning if this fails
        }
    }

    private string BuildConnectionString(string databaseName)
    {
        return BuildConnectionString(databaseName, _postgresHost, _postgresPort);
    }

    private string BuildConnectionString(string databaseName, string host, int port)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = _postgresUser,
            Password = _postgresPassword,
            Database = databaseName
        };
        return builder.ConnectionString;
    }

    private string BuildTenantConnectionString(string tenantDbName)
    {
        return BuildTenantConnectionString(tenantDbName, _postgresHost, _postgresPort);
    }

    private string BuildTenantConnectionString(string tenantDbName, string host, int port)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = _postgresUser,
            Password = _postgresPassword,
            Database = tenantDbName,
            SearchPath = "domain"  // Tenant databases use 'domain' schema
        };
        return builder.ConnectionString;
    }
}
