using Carmasters.Core.Application.Configuration;
using Carmasters.Core.Application.Database;
using Carmasters.Core.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using Dapper;
using System.Data.Common;

public class TenancyRepository : ITenancyRepository
{
    private readonly DbOptions _dbOptions;
    private readonly ILogger<TenancyRepository> _logger;

    public TenancyRepository(IOptions<DbOptions> dbOptions, ILogger<TenancyRepository> logger)
    {
        _dbOptions = dbOptions.Value;
        _logger = logger;
    }

    public async Task CreateTenantDatabase(string tenantName)
    {
        var userDbName = new MultiTenancyDbName(_dbOptions, tenantName);
        var templateDbName = new MultiTenancyDbName(_dbOptions, DbKind.Template);

        var dbCreateScript = $@"CREATE DATABASE ""{userDbName}"" TEMPLATE ""{templateDbName}""";

        using (var connection = await GetTenancyConnection())
        {
            await connection.ExecuteAsync(dbCreateScript);
            _logger.LogInformation("Created tenant database {DatabaseName} from template {TemplateName}", userDbName, templateDbName);
        }
    } 
    public async Task<Npgsql.NpgsqlConnection> GetTenancyConnection()
    {
        var tenancyDbName = new MultiTenancyDbName(_dbOptions, DbKind.Tenancy);
        var connectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = _dbOptions.Host,
            Port = _dbOptions.Port,
            Username = _dbOptions.UserId,
            Password = _dbOptions.Password,
            Database = tenancyDbName
        };

        var connection = new Npgsql.NpgsqlConnection(connectionBuilder.ToString());
        await connection.OpenAsync();
        return connection;
    }

    public async Task CreateTenantUser(string tenantName, string username, string hashedPassword, Guid employeeId)
    {
        // Create the user in the tenancy database, not the main database
        using (var connection = await GetTenancyConnection())
        {
            var profilePicture = System.IO.File.ReadAllBytes(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "default_admin.png"));

            await connection.ExecuteAsync(
                "INSERT INTO public.user (tenantname, employeeid, username, password, email, validated,profile_image) VALUES (@TenantName, @EmployeeId, @UserName, @Password, @Email, @Validated,@ProfileImage)",
                new
                {
                    TenantName = tenantName,
                    EmployeeId = employeeId,
                    UserName = username,
                    Password = hashedPassword,
                    Email = "demo@example.com",
                    Validated = true,
                    ProfileImage=profilePicture
                });
        }
    }

    public async Task UpdateTenantUser(string tenantName, Guid employeeId)
    {
        // Update user in the tenancy database
        using (var connection = await GetTenancyConnection())
        {
            await connection.ExecuteAsync(
                "UPDATE public.user SET employeeid = @EmployeeId WHERE tenantname = @TenantName",
                new
                {
                    EmployeeId = employeeId,
                    TenantName = tenantName
                });
        }
    }

    public async Task<Npgsql.NpgsqlConnection> GetTenantConnection(string tenantName)
    {
        var userDbName = new MultiTenancyDbName(_dbOptions, tenantName);
        var connectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = _dbOptions.Host,
            Port = _dbOptions.Port,
            Username = _dbOptions.UserId,
            Password = _dbOptions.Password,
            Database = userDbName
        };

        var connection = new Npgsql.NpgsqlConnection(connectionBuilder.ToString());
        await connection.OpenAsync();
        return connection;
    }

   
}