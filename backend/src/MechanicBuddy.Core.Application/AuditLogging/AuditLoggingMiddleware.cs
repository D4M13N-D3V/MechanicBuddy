using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MechanicBuddy.Core.Application.Database;
using MechanicBuddy.Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MechanicBuddy.Core.Application.AuditLogging
{
    /// <summary>
    /// Middleware that logs all API requests for audit purposes.
    /// </summary>
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLoggingMiddleware> _logger;

        private static readonly string[] ExcludedPaths =
        {
            "/health",
            "/swagger",
            "/favicon",
            "/_next",
            "/_framework"
        };

        private static readonly string[] ExcludedExtensions =
        {
            ".css",
            ".js",
            ".map",
            ".png",
            ".jpg",
            ".jpeg",
            ".gif",
            ".svg",
            ".ico",
            ".woff",
            ".woff2",
            ".ttf"
        };

        public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Skip excluded paths and static files
            if (ShouldSkip(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                await LogRequestAsync(context, stopwatch.ElapsedMilliseconds);
            }
        }

        private bool ShouldSkip(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;

            // Skip excluded paths
            foreach (var excluded in ExcludedPaths)
            {
                if (pathValue.StartsWith(excluded, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Skip static file extensions
            foreach (var ext in ExcludedExtensions)
            {
                if (pathValue.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private async Task LogRequestAsync(HttpContext context, long durationMs)
        {
            try
            {
                // Get the service scope from DbConnectionScopeMiddleware
                if (context.Items[DbConnectionProvider.ItemKey] is not IServiceScope scope)
                {
                    _logger.LogWarning("Audit logging: No service scope available");
                    return;
                }

                var repository = scope.ServiceProvider.GetService<IRepository>();
                if (repository == null)
                {
                    _logger.LogWarning("Audit logging: No repository available");
                    return;
                }

                var userName = context.User?.Identity?.Name ?? "anonymous";
                var employeeIdClaim = context.User?.FindFirst("employee_id")?.Value;
                var employeeId = Guid.TryParse(employeeIdClaim, out var id) ? id : (Guid?)null;

                var (resourceType, resourceId, actionDescription) = ParseEndpoint(
                    context.Request.Path,
                    context.Request.Method);

                var actionType = DetermineActionType(context.Request.Path, context.Request.Method);

                var auditLog = new AuditLog(
                    userName: userName,
                    employeeId: employeeId,
                    ipAddress: GetClientIpAddress(context),
                    userAgent: TruncateString(context.Request.Headers.UserAgent.ToString(), 500),
                    actionType: actionType,
                    httpMethod: context.Request.Method,
                    endpoint: TruncateString(context.Request.Path.Value ?? "/", 500),
                    resourceType: resourceType,
                    resourceId: resourceId,
                    actionDescription: TruncateString(actionDescription, 500),
                    statusCode: context.Response.StatusCode,
                    wasSuccessful: context.Response.StatusCode >= 200 && context.Response.StatusCode < 400,
                    durationMs: (int)durationMs);

                repository.Add(auditLog);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                _logger.LogError(ex, "Audit logging failed for request {Path}", context.Request.Path);
            }

            await Task.CompletedTask;
        }

        private string DetermineActionType(PathString path, string method)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;

            // Auth events
            if (pathValue.Contains("/users/login") ||
                pathValue.Contains("/users/logout") ||
                pathValue.Contains("/users/register") ||
                pathValue.Contains("/auth/"))
                return "auth";

            // Admin actions
            if (pathValue.Contains("/usermanagement") ||
                pathValue.Contains("/admin/") ||
                pathValue.Contains("/settings/"))
                return "admin";

            // CRUD operations (non-GET requests)
            if (method != "GET")
                return "crud";

            // Default API request
            return "api_request";
        }

        private (string resourceType, string resourceId, string description) ParseEndpoint(
            PathString path, string method)
        {
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries)
                ?? Array.Empty<string>();

            if (segments.Length < 2)
                return (null, null, $"{method} {path}");

            // segments[0] is typically "api", segments[1] is the resource type
            var resourceType = segments.Length > 1 ? segments[1].ToLowerInvariant() : null;

            // Check if segments[2] is a GUID (resource ID)
            string resourceId = null;
            if (segments.Length > 2 && Guid.TryParse(segments[2], out _))
            {
                resourceId = segments[2];
            }

            var action = method switch
            {
                "GET" => resourceId != null ? "Retrieved" : "Listed",
                "POST" => "Created",
                "PUT" => "Updated",
                "DELETE" => "Deleted",
                "PATCH" => "Modified",
                _ => method
            };

            var description = $"{action} {resourceType}";
            if (resourceId != null)
            {
                description += $" ({resourceId})";
            }

            return (resourceType, resourceId, description);
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded headers first (for proxied requests)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // X-Forwarded-For can contain multiple IPs, take the first one
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
