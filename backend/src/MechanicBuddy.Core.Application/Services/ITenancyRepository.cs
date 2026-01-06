using System;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;

namespace Carmasters.Core.Application.Services
{
    public interface ITenancyRepository
    {
        Task CreateTenantDatabase(string tenantName);
        Task CreateTenantUser(string tenantName, string username, string hashedPassword, Guid employeeId);
        Task<NpgsqlConnection> GetTenantConnection(string tenantName);
        Task UpdateTenantUser(string tenantName, Guid employeeId);
        Task<Npgsql.NpgsqlConnection> GetTenancyConnection();
    }
}