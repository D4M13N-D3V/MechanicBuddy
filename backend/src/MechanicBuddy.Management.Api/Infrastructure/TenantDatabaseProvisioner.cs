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
    private readonly string _postgresDatabase;

    // Default admin password hash (password: "carcare")
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
        _postgresDatabase = configuration["Database:PostgresDatabase"] ?? "mechanicbuddy";
    }

    public async Task<string> ProvisionTenantDatabaseAsync(string tenantId)
    {
        var schemaName = GetSchemaName(tenantId);
        var connectionString = BuildConnectionString();

        _logger.LogInformation("Provisioning database for tenant {TenantId} with schema {Schema}", tenantId, schemaName);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Create the tenant schema
        await CreateSchemaAsync(connection, schemaName);

        // Run all migrations
        await RunMigrationsAsync(connection, schemaName, tenantId);

        // Create the default admin user
        await CreateDefaultAdminAsync(connection, schemaName, tenantId);

        _logger.LogInformation("Successfully provisioned database for tenant {TenantId}", tenantId);

        return BuildTenantConnectionString(schemaName);
    }

    public async Task DeleteTenantDatabaseAsync(string tenantId)
    {
        var schemaName = GetSchemaName(tenantId);
        var connectionString = BuildConnectionString();

        _logger.LogInformation("Deleting database schema for tenant {TenantId}", tenantId);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Delete the user first (from public schema)
        await using (var cmd = new NpgsqlCommand(
            "DELETE FROM public.\"user\" WHERE tenantname = @tenantId", connection))
        {
            cmd.Parameters.AddWithValue("tenantId", tenantId);
            await cmd.ExecuteNonQueryAsync();
        }

        // Drop the domain schema
        await using (var cmd = new NpgsqlCommand(
            $"DROP SCHEMA IF EXISTS \"{schemaName}\" CASCADE", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Drop the tenant_config schema
        var configSchemaName = $"{schemaName}_config";
        await using (var cmd = new NpgsqlCommand(
            $"DROP SCHEMA IF EXISTS \"{configSchemaName}\" CASCADE", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        _logger.LogInformation("Deleted database schema for tenant {TenantId}", tenantId);
    }

    public async Task<bool> TenantDatabaseExistsAsync(string tenantId)
    {
        var schemaName = GetSchemaName(tenantId);
        var connectionString = BuildConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = @schemaName)", connection);
        cmd.Parameters.AddWithValue("schemaName", schemaName);

        var result = await cmd.ExecuteScalarAsync();
        return result is bool exists && exists;
    }

    private async Task CreateSchemaAsync(NpgsqlConnection connection, string schemaName)
    {
        // Create helper function if it doesn't exist
        await using (var cmd = new NpgsqlCommand(@"
            CREATE OR REPLACE FUNCTION f_concat_ws(text, VARIADIC text[])
            RETURNS text LANGUAGE sql IMMUTABLE AS 'SELECT array_to_string($2, $1)'", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Create the main schema (replaces 'domain' schema from template)
        await using (var cmd = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        _logger.LogDebug("Created schema {Schema}", schemaName);
    }

    private async Task RunMigrationsAsync(NpgsqlConnection connection, string schemaName, string tenantId)
    {
        // Set the search path to use tenant schema
        await using (var cmd = new NpgsqlCommand($"SET search_path TO \"{schemaName}\", public", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Script0000 - Create core tables (adapted from Script0000_createSchema.sql)
        await CreateCoreTablesAsync(connection, schemaName);

        // Script0002 - Ensure work number unique
        await EnsureWorkNumberUniqueAsync(connection, schemaName);

        // Script0003 - Create service request table
        await CreateServiceRequestTableAsync(connection, schemaName);

        _logger.LogDebug("Ran migrations for schema {Schema}", schemaName);
    }

    private async Task CreateCoreTablesAsync(NpgsqlConnection connection, string schemaName)
    {
        var sql = $@"
-- Employee table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".employee (
    id uuid primary key,
    firstname varchar NOT NULL,
    lastname varchar NOT NULL,
    email varchar,
    phone varchar,
    address varchar,
    proffession varchar,
    description varchar,
    introducedat timestamp without time zone not null
);

-- Vehicle table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".vehicle (
    id uuid primary key,
    producer VARCHAR,
    model VARCHAR,
    regnr VARCHAR not null,
    vin VARCHAR,
    odo INT,
    body varchar,
    drivingside varchar,
    engine varchar,
    productiondate date,
    region varchar,
    series varchar,
    transmission varchar,
    description varchar,
    introducedat timestamp with time zone not null
);

-- Client table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".client (
    id uuid primary key,
    address varchar,
    country varchar,
    region varchar,
    city varchar,
    postalcode varchar,
    phone varchar,
    description varchar,
    isasshole boolean default false NOT NULL,
    introducedat timestamp with time zone not null
);

-- VehicleRegistration table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".vehicleregistration (
    ownerid uuid NOT NULL references ""{schemaName}"".client,
    vehicleid uuid not null references ""{schemaName}"".vehicle,
    datetimefrom timestamp with time zone not null,
    datetimeto timestamp with time zone null,
    primary key (ownerid, vehicleid, datetimefrom)
);

-- ClientEmail table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".clientemail (
    address varchar not null,
    clientid uuid references ""{schemaName}"".client,
    isactive boolean not null,
    primary key (address, clientid)
);

-- PrivateClient table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".privateclient (
    id uuid PRIMARY KEY references ""{schemaName}"".client,
    firstname varchar NOT NULL,
    lastname varchar,
    personalcode varchar
);

-- LegalClient table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".legalclient (
    id uuid PRIMARY KEY references ""{schemaName}"".client,
    name varchar NOT NULL,
    regnr varchar
);

-- Pricing table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".pricing (
    id uuid primary key,
    senton timestamp with time zone,
    printedon timestamp with time zone,
    email varchar,
    partyname varchar not null,
    partyaddress varchar,
    partycode varchar,
    vehicleline1 varchar,
    vehicleline2 varchar,
    vehicleline3 varchar,
    vehicleline4 varchar,
    issuedon timestamp with time zone not null,
    issuerid uuid not null references ""{schemaName}"".employee
);

-- Estimate table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".estimate (
    id uuid primary key references ""{schemaName}"".pricing,
    number varchar not null unique
);

-- Invoice table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".invoice (
    id uuid primary key references ""{schemaName}"".pricing,
    number int not null unique,
    paymenttype smallint not null,
    duedays smallint not null,
    ispaid boolean default false not null,
    iscredited boolean default false NULL
);

-- Work table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".work (
    id uuid primary key,
    number int not null,
    invoiceid uuid references ""{schemaName}"".invoice,
    clientid uuid references ""{schemaName}"".client,
    vehicleid uuid null references ""{schemaName}"".vehicle,
    startedon timestamp with time zone not null,
    changedon timestamp with time zone not null unique,
    starterid uuid not null references ""{schemaName}"".employee,
    notes varchar,
    odo int,
    userstatus varchar default 'Default' NOT NULL,
    completedon timestamp with time zone,
    completerid uuid references ""{schemaName}"".employee
);

-- Offer table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".offer (
    id uuid primary key,
    workid uuid not null references ""{schemaName}"".work(id),
    ordernr smallint not null,
    notes varchar,
    estimateid uuid references ""{schemaName}"".estimate,
    isvehilelesonestimate boolean default false not null,
    startedon timestamp with time zone not null,
    starterid uuid not null references ""{schemaName}"".employee,
    acceptedon timestamp with time zone,
    acceptorid uuid references ""{schemaName}"".employee,
    unique(workid, ordernr)
);

-- RepairJob table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".repairjob (
    id uuid primary key,
    workid uuid not null references ""{schemaName}"".work(id),
    ordernr smallint not null,
    notes varchar,
    startedon timestamp with time zone not null,
    starterid uuid not null references ""{schemaName}"".employee,
    unique(workid, ordernr)
);

-- Assignment table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".assignment (
    workid uuid not null references ""{schemaName}"".work(id),
    mechanicid uuid not null references ""{schemaName}"".employee,
    primary key (workid, mechanicid)
);

-- Saleable table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".saleable (
    id uuid primary key,
    name varchar not null,
    quantity double precision not null,
    unit varchar not null,
    price double precision not null,
    discount smallint
);

-- ServiceOffered table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".serviceoffered (
    id uuid primary key references ""{schemaName}"".saleable,
    offerid uuid not null references ""{schemaName}"".offer(id)
);

-- ProductOffered table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".productoffered (
    id uuid primary key references ""{schemaName}"".saleable,
    offerid uuid not null references ""{schemaName}"".offer(id),
    code varchar not null,
    jnr smallint not null,
    serviceid uuid references ""{schemaName}"".serviceoffered
);

-- ServicePerformed table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".serviceperformed (
    id uuid primary key references ""{schemaName}"".saleable,
    repairjobid uuid not null references ""{schemaName}"".repairjob,
    notes varchar,
    mechanicid uuid references ""{schemaName}"".employee
);

-- ProductInstalled table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".productinstalled (
    id uuid primary key references ""{schemaName}"".saleable,
    repairjobid uuid not null references ""{schemaName}"".repairjob,
    jnr smallint not null,
    code varchar not null,
    notes varchar,
    status smallint not null,
    serviceid uuid references ""{schemaName}"".serviceperformed
);

-- Storage table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".storage (
    id uuid primary key,
    name varchar not null,
    address varchar,
    description varchar,
    introducedat timestamp with time zone not null
);

-- UnitedMotorsPrice table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".unitedmotorsprice (
    id uuid primary key,
    price double precision not null,
    name varchar NOT NULL,
    address varchar
);

-- SparePart table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".sparepart (
    id uuid primary key,
    code varchar not null,
    name varchar not null,
    price double precision,
    storageid uuid null references ""{schemaName}"".storage,
    quantity double precision,
    discount smallint,
    description varchar,
    introducedat timestamp with time zone not null,
    umpriceid uuid references ""{schemaName}"".unitedmotorsprice(id)
);

-- PricingLine table
CREATE TABLE IF NOT EXISTS ""{schemaName}"".pricingline (
    pricingid uuid not null references ""{schemaName}"".pricing,
    nr smallint not null,
    description varchar not null,
    quantity double precision not null,
    unitprice double precision not null,
    unit varchar not null,
    discount smallint not null default 0,
    total double precision not null,
    totalwithvat double precision not null,
    primary key (pricingid, nr)
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_vehicle_vin ON ""{schemaName}"".vehicle(vin);
CREATE INDEX IF NOT EXISTS idx_client_address ON ""{schemaName}"".client(address);
CREATE INDEX IF NOT EXISTS idx_client_phone ON ""{schemaName}"".client(phone);
CREATE INDEX IF NOT EXISTS idx_privateclient ON ""{schemaName}"".privateclient(firstname, lastname);
CREATE INDEX IF NOT EXISTS idx_pricing_issuerid ON ""{schemaName}"".pricing(issuerid);
CREATE INDEX IF NOT EXISTS idx_work_clientid ON ""{schemaName}"".work(clientid);
CREATE INDEX IF NOT EXISTS idx_work_starterid ON ""{schemaName}"".work(starterid);
CREATE INDEX IF NOT EXISTS idx_work_vehicleid ON ""{schemaName}"".work(vehicleid);
CREATE INDEX IF NOT EXISTS idx_offer_workid_ordernr ON ""{schemaName}"".offer(workid, ordernr);
CREATE INDEX IF NOT EXISTS idx_repairjob_workid_ordernr ON ""{schemaName}"".repairjob(workid, ordernr);
CREATE INDEX IF NOT EXISTS idx_offer_estimateid ON ""{schemaName}"".offer(estimateid);
CREATE INDEX IF NOT EXISTS idx_saleable_name ON ""{schemaName}"".saleable(name);
CREATE INDEX IF NOT EXISTS idx_productoffered_code ON ""{schemaName}"".productoffered(code);
CREATE INDEX IF NOT EXISTS idx_productinstalled_code ON ""{schemaName}"".productinstalled(code);
CREATE INDEX IF NOT EXISTS idx_number_work ON ""{schemaName}"".work(number);
CREATE INDEX IF NOT EXISTS idx_number_estimate ON ""{schemaName}"".estimate(number);
CREATE INDEX IF NOT EXISTS idx_number_invoice ON ""{schemaName}"".invoice(number);
";

        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();

        // Create tenant config schema and tables
        await CreateTenantConfigAsync(connection, schemaName);
    }

    private async Task CreateTenantConfigAsync(NpgsqlConnection connection, string schemaName)
    {
        var configSchemaName = $"{schemaName}_config";

        var sql = $@"
-- Create tenant config schema
CREATE SCHEMA IF NOT EXISTS ""{configSchemaName}"";

-- Create requisites table
CREATE TABLE IF NOT EXISTS ""{configSchemaName}"".requisites (
    id uuid PRIMARY KEY,
    name VARCHAR NOT NULL,
    phone VARCHAR,
    address VARCHAR,
    email VARCHAR,
    bank_account VARCHAR,
    reg_nr VARCHAR,
    tax_id VARCHAR,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create pricing table
CREATE TABLE IF NOT EXISTS ""{configSchemaName}"".pricing (
    id uuid PRIMARY KEY,
    vat_rate INTEGER NOT NULL DEFAULT 20,
    surcharge VARCHAR,
    disclaimer VARCHAR,
    signature_line BOOLEAN NOT NULL DEFAULT true,
    invoice_email_content TEXT,
    estimate_email_content TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Insert default values if they don't exist
INSERT INTO ""{configSchemaName}"".requisites (id, name, phone, address, email, bank_account, reg_nr, tax_id)
SELECT '6dd57256-2774-424f-a61b-887bf8327329', 'Default Company', '+1234567890', '123 Main St', 'info@example.com', 'EE123456789012', 'REG12345', 'VAT123456'
WHERE NOT EXISTS (SELECT 1 FROM ""{configSchemaName}"".requisites WHERE id = '6dd57256-2774-424f-a61b-887bf8327329');

INSERT INTO ""{configSchemaName}"".pricing (id, vat_rate, surcharge, disclaimer, signature_line, invoice_email_content, estimate_email_content)
SELECT '3b9806b3-287b-46cc-bc17-a2d40500327b', 20, 'Default Surcharge', 'Default Disclaimer', true,
       'Thank you for your business. Please find your invoice attached.',
       'Thank you for your interest. Please find your estimate attached.'
WHERE NOT EXISTS (SELECT 1 FROM ""{configSchemaName}"".pricing WHERE id = '3b9806b3-287b-46cc-bc17-a2d40500327b');
";

        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task EnsureWorkNumberUniqueAsync(NpgsqlConnection connection, string schemaName)
    {
        // Check if constraint already exists
        await using var checkCmd = new NpgsqlCommand($@"
            SELECT COUNT(*) FROM pg_constraint
            WHERE conname = 'work_number_key'
            AND conrelid = '""{schemaName}"".work'::regclass", connection);

        var exists = (long)(await checkCmd.ExecuteScalarAsync() ?? 0) > 0;
        if (exists) return;

        try
        {
            await using var cmd = new NpgsqlCommand(
                $@"ALTER TABLE ""{schemaName}"".work ADD CONSTRAINT work_number_key UNIQUE (number)", connection);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (PostgresException ex) when (ex.SqlState == "42710") // duplicate_object
        {
            // Constraint already exists, ignore
        }
    }

    private async Task CreateServiceRequestTableAsync(NpgsqlConnection connection, string schemaName)
    {
        var sql = $@"
CREATE TABLE IF NOT EXISTS ""{schemaName}"".servicerequest (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customername VARCHAR(255) NOT NULL,
    phone VARCHAR(50),
    email VARCHAR(255),
    vehicleinfo VARCHAR(500),
    servicetype VARCHAR(100),
    message TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'New',
    submittedat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    notes TEXT
);

CREATE INDEX IF NOT EXISTS idx_servicerequest_status ON ""{schemaName}"".servicerequest(status);
CREATE INDEX IF NOT EXISTS idx_servicerequest_submittedat ON ""{schemaName}"".servicerequest(submittedat DESC);
";

        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task CreateDefaultAdminAsync(NpgsqlConnection connection, string schemaName, string tenantId)
    {
        // First create the employee record
        var employeeId = Guid.NewGuid();

        await using (var cmd = new NpgsqlCommand($@"
            INSERT INTO ""{schemaName}"".employee (id, firstname, lastname, email, phone, proffession, description, introducedat)
            VALUES (@id, 'System', 'Administrator', 'admin@example.com', '', 'Administrator', 'Default system administrator', CURRENT_TIMESTAMP)", connection))
        {
            cmd.Parameters.AddWithValue("id", employeeId);
            await cmd.ExecuteNonQueryAsync();
        }

        // Ensure public.user table exists (it should be in the template database)
        await using (var cmd = new NpgsqlCommand(@"
            CREATE TABLE IF NOT EXISTS public.""user"" (
                username varchar NOT NULL,
                password varchar NOT NULL,
                tenantname varchar NOT NULL,
                email varchar NULL,
                validated boolean NOT NULL DEFAULT false,
                profile_image bytea null,
                employeeid uuid,
                PRIMARY KEY (username, tenantname)
            )", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

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

        // Create the admin user
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

        _logger.LogInformation("Created default admin user for tenant {TenantId}", tenantId);
    }

    private static string GetSchemaName(string tenantId)
    {
        // Convert tenant ID to valid schema name (e.g., "testt" -> "tenant_testt")
        return $"tenant_{tenantId.Replace("-", "_")}";
    }

    private string BuildConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _postgresHost,
            Port = _postgresPort,
            Username = _postgresUser,
            Password = _postgresPassword,
            Database = _postgresDatabase
        };
        return builder.ConnectionString;
    }

    private string BuildTenantConnectionString(string schemaName)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _postgresHost,
            Port = _postgresPort,
            Username = _postgresUser,
            Password = _postgresPassword,
            Database = _postgresDatabase,
            SearchPath = schemaName
        };
        return builder.ConnectionString;
    }
}
