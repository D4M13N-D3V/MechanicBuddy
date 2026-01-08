using System;
using System.Data.Common;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MechanicBuddy.Core.Application.Configuration;
using MechanicBuddy.Core.Application.Database;
using MechanicBuddy.Core.Application.Extensions;
using MechanicBuddy.Core.Application.RateLimiting;

namespace MechanicBuddy.Http.Api.Controllers
{
    /// <summary>
    /// Controller for viewing audit logs. Only accessible by default admin users.
    /// </summary>
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogsController : ControllerBase
    {
        private readonly DbOptions _dbOptions;

        public AuditLogsController(IOptions<DbOptions> dbOptions)
        {
            _dbOptions = dbOptions.Value;
        }

        /// <summary>
        /// Get paginated audit logs with filtering options.
        /// Only accessible by default admin users.
        /// </summary>
        [HttpGet]
        public ActionResult<AuditLogPageResult> GetPage(
            [FromQuery] string searchText,
            [FromQuery] string actionType,
            [FromQuery] string resourceType,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0)
        {
            if (!IsDefaultAdmin())
            {
                return Forbid();
            }

            using var connection = CreateConnection();

            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(searchText))
            {
                whereClause += @" AND (LOWER(user_name) LIKE @SearchText
                                    OR LOWER(endpoint) LIKE @SearchText
                                    OR LOWER(action_description) LIKE @SearchText)";
                parameters.Add("SearchText", $"%{searchText.ToLower()}%");
            }

            if (!string.IsNullOrEmpty(actionType))
            {
                whereClause += " AND action_type = @ActionType";
                parameters.Add("ActionType", actionType);
            }

            if (!string.IsNullOrEmpty(resourceType))
            {
                whereClause += " AND resource_type = @ResourceType";
                parameters.Add("ResourceType", resourceType);
            }

            if (fromDate.HasValue)
            {
                whereClause += " AND timestamp >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }

            if (toDate.HasValue)
            {
                whereClause += " AND timestamp <= @ToDate";
                parameters.Add("ToDate", toDate.Value);
            }

            parameters.Add("Limit", limit);
            parameters.Add("Offset", offset);

            var countQuery = $"SELECT COUNT(*) FROM domain.audit_logs {whereClause}";
            var total = connection.QuerySingle<int>(countQuery, parameters);

            var selectQuery = $@"
                SELECT
                    id as Id,
                    user_name as UserName,
                    employee_id as EmployeeId,
                    ip_address as IpAddress,
                    user_agent as UserAgent,
                    action_type as ActionType,
                    http_method as HttpMethod,
                    endpoint as Endpoint,
                    resource_type as ResourceType,
                    resource_id as ResourceId,
                    action_description as ActionDescription,
                    timestamp as Timestamp,
                    duration_ms as DurationMs,
                    status_code as StatusCode,
                    was_successful as WasSuccessful
                FROM domain.audit_logs
                {whereClause}
                ORDER BY timestamp DESC
                LIMIT @Limit OFFSET @Offset";

            var items = connection.Query<AuditLogDto>(selectQuery, parameters).ToArray();

            return Ok(new AuditLogPageResult
            {
                Items = items,
                Total = total,
                HasMore = (offset + limit) < total
            });
        }

        /// <summary>
        /// Get audit log statistics for the last N days.
        /// Only accessible by default admin users.
        /// </summary>
        [HttpGet("stats")]
        public ActionResult<AuditLogStats> GetStats([FromQuery] int days = 7)
        {
            if (!IsDefaultAdmin())
            {
                return Forbid();
            }

            using var connection = CreateConnection();
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var stats = connection.QuerySingle<AuditLogStats>(@"
                SELECT
                    COUNT(*) as TotalRequests,
                    COUNT(DISTINCT user_name) as UniqueUsers,
                    COUNT(*) FILTER (WHERE action_type = 'crud') as CrudOperations,
                    COUNT(*) FILTER (WHERE action_type = 'auth') as AuthEvents,
                    COUNT(*) FILTER (WHERE was_successful = false) as FailedRequests
                FROM domain.audit_logs
                WHERE timestamp >= @FromDate",
                new { FromDate = fromDate });

            return Ok(stats);
        }

        /// <summary>
        /// Check if the current user can view audit logs.
        /// </summary>
        [HttpGet("canview")]
        public ActionResult<CanViewAuditLogsDto> CanView()
        {
            return Ok(new CanViewAuditLogsDto
            {
                CanView = IsDefaultAdmin()
            });
        }

        /// <summary>
        /// Checks if the current user is the default admin.
        /// </summary>
        private bool IsDefaultAdmin()
        {
            var tenantName = this.TenantName();
            using var connection = CreateConnection();

            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return false;

            var isDefaultAdmin = connection.QuerySingleOrDefault<bool?>(@"
                SELECT is_default_admin
                FROM public.user
                WHERE tenantname = @TenantName AND username = @UserName",
                new { TenantName = tenantName, UserName = userName });

            return isDefaultAdmin ?? false;
        }

        /// <summary>
        /// Creates a database connection.
        /// </summary>
        private DbConnection CreateConnection()
        {
            var databaseName = _dbOptions.MultiTenancy?.Enabled == true
                ? new MultiTenancyDbName(_dbOptions, DbKind.Tenancy)
                : _dbOptions.Name;

            var connectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = _dbOptions.Host,
                Port = _dbOptions.Port,
                Username = _dbOptions.UserId,
                Password = _dbOptions.Password,
                Database = databaseName
            };

            var connection = new Npgsql.NpgsqlConnection(connectionBuilder.ToString());
            connection.Open();
            return connection;
        }
    }

    /// <summary>
    /// DTO for audit log entries.
    /// </summary>
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public Guid? EmployeeId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string ActionType { get; set; }
        public string HttpMethod { get; set; }
        public string Endpoint { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public string ActionDescription { get; set; }
        public DateTime Timestamp { get; set; }
        public int? DurationMs { get; set; }
        public int StatusCode { get; set; }
        public bool WasSuccessful { get; set; }
    }

    /// <summary>
    /// Paginated result for audit logs.
    /// </summary>
    public class AuditLogPageResult
    {
        public AuditLogDto[] Items { get; set; }
        public int Total { get; set; }
        public bool HasMore { get; set; }
    }

    /// <summary>
    /// Statistics for audit logs.
    /// </summary>
    public class AuditLogStats
    {
        public int TotalRequests { get; set; }
        public int UniqueUsers { get; set; }
        public int CrudOperations { get; set; }
        public int AuthEvents { get; set; }
        public int FailedRequests { get; set; }
    }

    /// <summary>
    /// Response for can view check.
    /// </summary>
    public class CanViewAuditLogsDto
    {
        public bool CanView { get; set; }
    }
}
