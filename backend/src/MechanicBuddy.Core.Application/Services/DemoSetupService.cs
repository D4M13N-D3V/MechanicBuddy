using System;
using System.Threading.Tasks;
using Carmasters.Core.Application.Authorization;
using Carmasters.Core.Application.Configuration;
using Carmasters.Core.Application.Database;
using Carmasters.Core.Application.Model;
using Carmasters.Core.Domain;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Carmasters.Core.Application.Services
{
    public interface IDemoSetupService
    {
        Task<(string username, string password, string tenantName)> CreateDemoTenant(string companyName = null);
        Task PopulateDemoData(string tenantName, string companyName, Guid adminEmployeeId);
    }

    public class DemoSetupService : IDemoSetupService
    {
        private readonly ILogger<DemoSetupService> _logger;
        private readonly DbOptions _dbOptions;
        private readonly ITenancyRepository _repository;

        public DemoSetupService(
            ILogger<DemoSetupService> logger,
            IOptions<DbOptions> dbOptions,
            ITenancyRepository repository)
        {
            _logger = logger;
            _dbOptions = dbOptions.Value;
            _repository = repository;
        }
        public async Task<(string username, string password, string tenantName)> CreateDemoTenant(string companyName = null)
        {
            if (_dbOptions.MultiTenancy?.Enabled != true)
            {
                throw new InvalidOperationException("Multi-tenancy must be enabled for demo setup");
            }

            string tenantName = ShortGuid.NewGuid();
            string username = $"demo{tenantName}";
            string password = "carcare";
            string hashedPassword = Authorization.PasswordHasher.getHash(password);

            // Create tenant database from template
            await _repository.CreateTenantDatabase(tenantName);

            // Create admin employee and user
            var adminEmployeeId = Guid.NewGuid();

            // Create user in tenancy database
            await _repository.CreateTenantUser(tenantName, username, hashedPassword, adminEmployeeId);

            // Populate demo data
            await PopulateDemoData(tenantName, companyName ?? "Demo Company", adminEmployeeId);

            return (username, password, tenantName);
        }

        public async Task PopulateDemoData(string tenantName, string companyName, Guid adminEmployeeId)
        {

            await using (var connection = await _repository.GetTenantConnection(tenantName))
            {
                // Create admin employee
                await connection.ExecuteAsync(@"
                INSERT INTO domain.employee(id, firstname, lastname, email, phone, proffession, description, introducedat)
                VALUES (@Id, 'Demo', 'Admin', 'admin@example.com', '+1234567890', 'Administrator', 'Demo Admin Account', CURRENT_TIMESTAMP)",
                    new { Id = adminEmployeeId });

                // Update the user with the employee ID
                await _repository.UpdateTenantUser(tenantName, adminEmployeeId);

                await connection.ExecuteAsync(@"
                    UPDATE tenant_config.requisites 
                    SET 
                        name = @CompanyName, 
                        updated_at = CURRENT_TIMESTAMP",
                  new { CompanyName = companyName ?? "Demo Company" });

                // Create storage
                var storageId = Guid.NewGuid();
                await connection.ExecuteAsync(@"
                    INSERT INTO domain.storage(id, name, address, description, introducedat)
                    VALUES (@Id, 'Main Storage', 'Storage Address', 'Main storage facility', CURRENT_TIMESTAMP)",
                    new { Id = storageId });

                // Create clients - one company and one individual
                var companyClientId = Guid.NewGuid();
                await connection.ExecuteAsync(@"
                    INSERT INTO domain.client(id, address, country, region, city, postalcode, phone, isasshole, description, introducedat)
                    VALUES (@Id, 'Main Street 123', 'USA', 'CA', 'San Francisco', '94105', '+1987654321', false, 'Demo Company Client', CURRENT_TIMESTAMP)",
                    new { Id = companyClientId });

                await connection.ExecuteAsync(@"
                    INSERT INTO domain.legalclient(id, name, regnr)
                    VALUES (@Id, @Name, 'REG12345')",
                    new { Id = companyClientId, Name = companyName });

                await connection.ExecuteAsync(@"
                    INSERT INTO domain.clientemail(clientid, address, isactive)
                    VALUES (@ClientId, 'company@example.com', true)",
                    new { ClientId = companyClientId });

                var privateClientId = Guid.NewGuid();
                await connection.ExecuteAsync(@"
                    INSERT INTO domain.client(id, address, country, region, city, postalcode, phone, isasshole, description, introducedat)
                    VALUES (@Id, 'Side Street 456', 'USA', 'CA', 'San Francisco', '94107', '+1567890123', false, 'Demo Private Client', CURRENT_TIMESTAMP)",
                    new { Id = privateClientId });

                await connection.ExecuteAsync(@"
                    INSERT INTO domain.privateclient(id, firstname, lastname, personalcode)
                    VALUES (@Id, 'John', 'Doe', 'ID12345')",
                    new { Id = privateClientId });

                await connection.ExecuteAsync(@"
                    INSERT INTO domain.clientemail(clientid, address, isactive)
                    VALUES (@ClientId, 'john.doe@example.com', true)",
                    new { ClientId = privateClientId });

                // Create vehicles
                var vehicleId1 = Guid.NewGuid();
                await connection.ExecuteAsync(@"
                    INSERT INTO domain.vehicle(id, producer, model, regnr, vin, odo, body, engine, productiondate, introducedat)
                    VALUES (@Id, 'Toyota', 'Corolla', 'ABC123', 'VIN12345678901234', 50000, 'Sedan', '1.8L', '2020-01-01', CURRENT_TIMESTAMP)",
                    new { Id = vehicleId1 });

                await connection.ExecuteAsync(@"
                    INSERT INTO domain.vehicleregistration(vehicleid, ownerid, datetimefrom)
                    VALUES (@VehicleId, @OwnerId, CURRENT_TIMESTAMP)",
                    new { VehicleId = vehicleId1, OwnerId = companyClientId });

                var vehicleId2 = Guid.NewGuid();
                await connection.ExecuteAsync(@"
                    INSERT INTO domain.vehicle(id, producer, model, regnr, vin, odo, body, engine, productiondate, introducedat)
                    VALUES (@Id, 'Honda', 'Civic', 'XYZ789', 'VIN98765432109876', 30000, 'Hatchback', '1.5L', '2021-05-15', CURRENT_TIMESTAMP)",
                    new { Id = vehicleId2 });

                await connection.ExecuteAsync(@"
                    INSERT INTO domain.vehicleregistration(vehicleid, ownerid, datetimefrom)
                    VALUES (@VehicleId, @OwnerId, CURRENT_TIMESTAMP)",
                    new { VehicleId = vehicleId2, OwnerId = privateClientId });

                // Create spare parts
                string[] partNames = {
                    "Oil Filter", "Air Filter", "Fuel Filter", "Brake Pads", "Spark Plugs",
                    "Wiper Blades", "Timing Belt", "Battery", "Radiator", "Alternator"
                };

                for (int i = 0; i < partNames.Length; i++)
                {
                    var partId = Guid.NewGuid();
                    await connection.ExecuteAsync(@"
                        INSERT INTO domain.sparepart(id, code, name, price, quantity, discount, storageid, description, introducedat)
                        VALUES (@Id, @Code, @Name, @Price, @Quantity, 0, @StorageId, @Description, CURRENT_TIMESTAMP)",
                        new
                        {
                            Id = partId,
                            Code = $"PART{i + 1:D3}",
                            Name = partNames[i],
                            Price = 10.0m + (i * 5.0m),
                            Quantity = 10 + i,
                            StorageId = storageId,
                            Description = $"Demo {partNames[i]}"
                        });
                }

                // Create mechanics
                var mechanic1Id = Guid.NewGuid();
                await connection.ExecuteAsync(@"
                    INSERT INTO domain.employee(id, firstname, lastname, email, phone, proffession, description, introducedat)
                    VALUES (@Id, 'Mike', 'Smith', 'mike.smith@example.com', '+1111222333', 'Senior Mechanic', 'Experienced mechanic', CURRENT_TIMESTAMP)",
                    new { Id = mechanic1Id });

                var mechanic2Id = Guid.NewGuid();
                await connection.ExecuteAsync(@"
                    INSERT INTO domain.employee(id, firstname, lastname, email, phone, proffession, description, introducedat)
                    VALUES (@Id, 'Emily', 'Johnson', 'emily.johnson@example.com', '+1444555666', 'Junior Mechanic', 'New mechanic', CURRENT_TIMESTAMP)",
                    new { Id = mechanic2Id });

                // Create works with offers and jobs
                await CreateSampleWork(connection, companyClientId, vehicleId1, adminEmployeeId, mechanic1Id, mechanic2Id);
                await CreateSampleWork(connection, privateClientId, vehicleId2, adminEmployeeId, mechanic1Id, null);
            }


        }

        private async Task CreateSampleWork(
          Npgsql.NpgsqlConnection connection,
          Guid clientId,
          Guid vehicleId,
          Guid adminEmployeeId,
          Guid? mechanic1Id,
          Guid? mechanic2Id)
        {
            // Create a work record
            var workId = Guid.NewGuid();
            var workNumber = new Random().Next(1000, 9999);

            await connection.ExecuteAsync(@"
        INSERT INTO domain.work(id, number, startedon, starterid, clientid, vehicleid, notes, odo, userstatus, changedon)
        VALUES (@Id, @Number, CURRENT_TIMESTAMP, @StarterId, @ClientId, @VehicleId, 'Demo work record', 55000, 0, CURRENT_TIMESTAMP)",
                new
                {
                    Id = workId,
                    Number = workNumber,
                    StarterId = adminEmployeeId,
                    ClientId = clientId,
                    VehicleId = vehicleId
                });

            // Assign mechanics to the work
            if (mechanic1Id.HasValue)
            {
                await connection.ExecuteAsync(@"
            INSERT INTO domain.assignment(workid, mechanicid)
            VALUES (@WorkId, @MechanicId)",
                    new { WorkId = workId, MechanicId = mechanic1Id.Value });
            }

            if (mechanic2Id.HasValue)
            {
                await connection.ExecuteAsync(@"
            INSERT INTO domain.assignment(workid, mechanicid)
            VALUES (@WorkId, @MechanicId)",
                    new { WorkId = workId, MechanicId = mechanic2Id.Value });
            }

            // Create an offer
            var offerId = Guid.NewGuid();
            await connection.ExecuteAsync(@"
        INSERT INTO domain.offer(id, workid, ordernr, notes, isvehilelinesonestimate, startedon, starterid)
        VALUES (@Id, @WorkId, 1, 'Demo offer', true, CURRENT_TIMESTAMP, @StarterId)",
                new { Id = offerId, WorkId = workId, StarterId = adminEmployeeId });

            // Add products to the offer
            var product1Id = Guid.NewGuid();
            await connection.ExecuteAsync(@"
        INSERT INTO domain.saleable(id, name, quantity, unit, price, discount)
        VALUES (@Id, @Name, @Quantity, @Unit, @Price, @Discount)",
                new
                {
                    Id = product1Id,
                    Name = "Oil Change Service",
                    Quantity = 1.0,
                    Unit = "pcs",
                    Price = 49.99,
                    Discount = (short)0
                });

            await connection.ExecuteAsync(@"
        INSERT INTO domain.productoffered(id, offerid, jnr, code)
        VALUES (@Id, @OfferId, @Jnr, @Code)",
                new
                {
                    Id = product1Id,
                    OfferId = offerId,
                    Jnr = (short)1,
                    Code = "SERVICE001"
                });

            var product2Id = Guid.NewGuid();
            await connection.ExecuteAsync(@"
        INSERT INTO domain.saleable(id, name, quantity, unit, price, discount)
        VALUES (@Id, @Name, @Quantity, @Unit, @Price, @Discount)",
                new
                {
                    Id = product2Id,
                    Name = "Brake Inspection",
                    Quantity = 1.0,
                    Unit = "pcs",
                    Price = 29.99,
                    Discount = (short)0
                });

            await connection.ExecuteAsync(@"
        INSERT INTO domain.productoffered(id, offerid, jnr, code)
        VALUES (@Id, @OfferId, @Jnr, @Code)",
                new
                {
                    Id = product2Id,
                    OfferId = offerId,
                    Jnr = (short)2,
                    Code = "SERVICE002"
                });

            // Create a repair job
            var jobId = Guid.NewGuid();
            await connection.ExecuteAsync(@"
        INSERT INTO domain.repairjob(id, workid, ordernr, notes, startedon, starterid)
        VALUES (@Id, @WorkId, @OrderNr, @Notes, CURRENT_TIMESTAMP, @StarterId)",
                new
                {
                    Id = jobId,
                    WorkId = workId,
                    OrderNr = (short)1,
                    Notes = "Demo repair job",
                    StarterId = adminEmployeeId
                });

            // Add products to the repair job
            var installed1Id = Guid.NewGuid();
            await connection.ExecuteAsync(@"
        INSERT INTO domain.saleable(id, name, quantity, unit, price, discount)
        VALUES (@Id, @Name, @Quantity, @Unit, @Price, @Discount)",
                new
                {
                    Id = installed1Id,
                    Name = "Oil Filter Replacement",
                    Quantity = 1.0,
                    Unit = "pcs",
                    Price = 19.99,
                    Discount = (short)0
                });

            await connection.ExecuteAsync(@"
        INSERT INTO domain.productinstalled(id, repairjobid, jnr, code, status, notes)
        VALUES (@Id, @JobId, @Jnr, @Code, @Status, @Notes)",
                new
                {
                    Id = installed1Id,
                    JobId = jobId,
                    Jnr = (short)1,
                    Code = "PART001",
                    Status = (short)3,
                    Notes = "Installed new oil filter"
                });

            var installed2Id = Guid.NewGuid();
            await connection.ExecuteAsync(@"
        INSERT INTO domain.saleable(id, name, quantity, unit, price, discount)
        VALUES (@Id, @Name, @Quantity, @Unit, @Price, @Discount)",
                new
                {
                    Id = installed2Id,
                    Name = "Engine Oil 5W-30",
                    Quantity = 5.0,
                    Unit = "L",
                    Price = 9.99,
                    Discount = (short)0
                });

            await connection.ExecuteAsync(@"
        INSERT INTO domain.productinstalled(id, repairjobid, jnr, code, status, notes)
        VALUES (@Id, @JobId, @Jnr, @Code, @Status, @Notes)",
                new
                {
                    Id = installed2Id,
                    JobId = jobId,
                    Jnr = (short)2,
                    Code = "PART002",
                    Status = (short)3,
                    Notes = "Used 5 liters of oil"
                });

            // Create an estimate
            var estimateId = Guid.NewGuid();
            var estimateNumber = $"{workNumber}-1";

            await connection.ExecuteAsync(@"
        INSERT INTO domain.pricing(id, issuedon, issuerid, partyname, vehicleline1, vehicleline2)
        VALUES (@Id, CURRENT_TIMESTAMP, @IssuerId, @PartyName, @VehicleLine1, @VehicleLine2)",
                new
                {
                    Id = estimateId,
                    IssuerId = adminEmployeeId,
                    PartyName = "Demo Client",
                    VehicleLine1 = "Vehicle: Demo Vehicle",
                    VehicleLine2 = "Reg nr: ABC123"
                });

            await connection.ExecuteAsync(@"
        INSERT INTO domain.estimate(id, number)
        VALUES (@Id, @Number)",
                new { Id = estimateId, Number = estimateNumber });

            // Link estimate to the offer
            await connection.ExecuteAsync(@"
        UPDATE domain.offer SET estimateid = @EstimateId WHERE id = @OfferId",
                new { EstimateId = estimateId, OfferId = offerId });

            // Add pricing lines
            await connection.ExecuteAsync(@"
        INSERT INTO domain.pricingline(pricingid, nr, description, quantity, unitprice, unit, discount, total, totalwithvat)
        VALUES (@PricingId, @Nr, @Description, @Quantity, @UnitPrice, @Unit, @Discount, @Total, @TotalWithVat)",
                new
                {
                    PricingId = estimateId,
                    Nr = (short)1,
                    Description = "Oil Change Service",
                    Quantity = 1.0,
                    UnitPrice = 49.99,
                    Unit = "pcs",
                    Discount = (short)0,
                    Total = 49.99,
                    TotalWithVat = 59.99
                });

            await connection.ExecuteAsync(@"
        INSERT INTO domain.pricingline(pricingid, nr, description, quantity, unitprice, unit, discount, total, totalwithvat)
        VALUES (@PricingId, @Nr, @Description, @Quantity, @UnitPrice, @Unit, @Discount, @Total, @TotalWithVat)",
                new
                {
                    PricingId = estimateId,
                    Nr = (short)2,
                    Description = "Brake Inspection",
                    Quantity = 1.0,
                    UnitPrice = 29.99,
                    Unit = "pcs",
                    Discount = (short)0,
                    Total = 29.99,
                    TotalWithVat = 35.99
                });
        } 
         
    }
}