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

    // Default admin password hash (password: "carcare")
    private const string DefaultAdminPasswordHash = "$2a$11$eXvWgMFQ2S5C5VZGTH.WKO/GzsKzRBXhgaNGcFJovWjag3wUPmukC";

    // Template employee ID (from mechanicbuddy-testt template database)
    private const string TemplateEmployeeId = "a7227c1f-3bbc-4367-a9b8-baa83d0f19ca";

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
        var tenantDbName = GetTenantDbName(tenantId);

        _logger.LogInformation("Provisioning database {DbName} for tenant {TenantId} from template {Template}",
            tenantDbName, tenantId, _templateDbName);

        // Connect to postgres database to create new database
        var adminConnectionString = BuildConnectionString("postgres");
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
        await UpdateTenantUserTableAsync(tenantId, tenantDbName);

        // Create admin user in tenancy database
        await CreateDefaultAdminAsync(tenantId);

        _logger.LogInformation("Successfully provisioned database for tenant {TenantId}", tenantId);

        return BuildTenantConnectionString(tenantDbName);
    }

    public async Task DeleteTenantDatabaseAsync(string tenantId)
    {
        var tenantDbName = GetTenantDbName(tenantId);

        _logger.LogInformation("Deleting database {DbName} for tenant {TenantId}", tenantDbName, tenantId);

        // First, delete the user from tenancy database
        var tenancyConnectionString = BuildConnectionString(_tenancyDbName);
        await using (var tenancyConnection = new NpgsqlConnection(tenancyConnectionString))
        {
            await tenancyConnection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "DELETE FROM public.\"user\" WHERE tenantname = @tenantId", tenancyConnection);
            cmd.Parameters.AddWithValue("tenantId", tenantId);
            await cmd.ExecuteNonQueryAsync();
        }

        // Then drop the database (connect to postgres database to do this)
        var adminConnectionString = BuildConnectionString("postgres");
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

        _logger.LogInformation("Deleted database {DbName} for tenant {TenantId}", tenantDbName, tenantId);
    }

    public async Task<bool> TenantDatabaseExistsAsync(string tenantId)
    {
        var tenantDbName = GetTenantDbName(tenantId);
        var adminConnectionString = BuildConnectionString("postgres");

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

    private async Task UpdateTenantUserTableAsync(string tenantId, string tenantDbName)
    {
        // Update the user table in the cloned tenant database to use the correct tenant name
        // The template database has users with the template's tenant name, we need to update them
        var tenantConnectionString = BuildConnectionString(tenantDbName);

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

    private async Task CreateDefaultAdminAsync(string tenantId)
    {
        // Create admin user in the tenancy database (where all users are stored)
        // The employee record already exists in the cloned tenant database from the template
        var tenancyConnectionString = BuildConnectionString(_tenancyDbName);
        var employeeId = Guid.Parse(TemplateEmployeeId);

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

        // Create the admin user with the template employee ID
        await using (var cmd = new NpgsqlCommand(@"
            INSERT INTO public.""user"" (username, password, tenantname, email, validated, profile_image, employeeid)
            VALUES (@username, @password, @tenantname, @email, @validated, @profileImage, @employeeId)", connection))
        {
            cmd.Parameters.AddWithValue("username", "admin");
            cmd.Parameters.AddWithValue("password", DefaultAdminPasswordHash);
            cmd.Parameters.AddWithValue("tenantname", tenantId);
            cmd.Parameters.AddWithValue("email", "admin@example.com");
            cmd.Parameters.AddWithValue("validated", true);
            cmd.Parameters.AddWithValue("profileImage", DBNull.Value);
            cmd.Parameters.AddWithValue("employeeId", employeeId);
            await cmd.ExecuteNonQueryAsync();
        }

        _logger.LogInformation("Created default admin user for tenant {TenantId} with employee ID {EmployeeId}", tenantId, employeeId);
    }

    private string BuildConnectionString(string databaseName)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _postgresHost,
            Port = _postgresPort,
            Username = _postgresUser,
            Password = _postgresPassword,
            Database = databaseName
        };
        return builder.ConnectionString;
    }

    private string BuildTenantConnectionString(string tenantDbName)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _postgresHost,
            Port = _postgresPort,
            Username = _postgresUser,
            Password = _postgresPassword,
            Database = tenantDbName,
            SearchPath = "domain"  // Tenant databases use 'domain' schema
        };
        return builder.ConnectionString;
    }
}
