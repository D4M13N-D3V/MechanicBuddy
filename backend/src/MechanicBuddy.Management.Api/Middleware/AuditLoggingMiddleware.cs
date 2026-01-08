using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;

namespace MechanicBuddy.Management.Api.Middleware;

/// <summary>
/// Middleware that logs all API requests for audit purposes in the Management API.
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    private static readonly string[] ExcludedPaths =
    {
        "/health",
        "/swagger",
        "/favicon"
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
        ".ico"
    };

    // Regex to extract tenant ID from common paths like /api/tenants/{tenantId} or /tenants/{tenantId}
    private static readonly Regex TenantIdRegex = new Regex(
        @"/(?:api/)?tenants/([^/]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
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
            var repository = context.RequestServices.GetService<IAuditLogRepository>();
            if (repository == null)
            {
                _logger.LogWarning("Audit logging: No repository available");
                return;
            }

            // Extract admin info from claims
            var adminIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminId = int.TryParse(adminIdClaim, out var id) ? id : (int?)null;
            var adminEmail = context.User?.FindFirst(ClaimTypes.Email)?.Value
                ?? context.User?.FindFirst("email")?.Value
                ?? "anonymous";
            var adminRole = context.User?.FindFirst(ClaimTypes.Role)?.Value
                ?? context.User?.FindFirst("role")?.Value;

            var (resourceType, resourceId, tenantId, actionDescription) = ParseEndpoint(
                context.Request.Path,
                context.Request.Method);

            var actionType = DetermineActionType(context.Request.Path, context.Request.Method);

            var auditLog = new AuditLog
            {
                AdminId = adminId,
                AdminEmail = TruncateString(adminEmail, 255),
                AdminRole = TruncateString(adminRole, 50),
                IpAddress = TruncateString(GetClientIpAddress(context), 50),
                UserAgent = TruncateString(context.Request.Headers.UserAgent.ToString(), 500),
                ActionType = actionType,
                HttpMethod = context.Request.Method,
                Endpoint = TruncateString(context.Request.Path.Value ?? "/", 500),
                ResourceType = TruncateString(resourceType, 100),
                ResourceId = TruncateString(resourceId, 100),
                TenantId = TruncateString(tenantId, 50),
                ActionDescription = TruncateString(actionDescription, 500),
                Timestamp = DateTime.UtcNow,
                DurationMs = (int)durationMs,
                StatusCode = context.Response.StatusCode,
                WasSuccessful = context.Response.StatusCode >= 200 && context.Response.StatusCode < 400
            };

            await repository.CreateAsync(auditLog);
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request
            _logger.LogError(ex, "Audit logging failed for request {Path}", context.Request.Path);
        }
    }

    private string DetermineActionType(PathString path, string method)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;

        // Auth events
        if (pathValue.Contains("/auth/") ||
            pathValue.Contains("/login") ||
            pathValue.Contains("/logout") ||
            pathValue.Contains("/signup"))
            return "auth";

        // Admin management
        if (pathValue.Contains("/superadmin") ||
            pathValue.Contains("/admin"))
            return "admin";

        // Tenant operations
        if (pathValue.Contains("/tenants") ||
            pathValue.Contains("/provisioning") ||
            pathValue.Contains("/demos"))
            return "tenant_operation";

        // Default API request
        return "api_request";
    }

    private (string? resourceType, string? resourceId, string? tenantId, string description) ParseEndpoint(
        PathString path, string method)
    {
        var pathValue = path.Value ?? "/";
        var segments = pathValue.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 1)
            return (null, null, null, $"{method} {path}");

        // Determine resource type from path
        string? resourceType = null;
        if (segments.Length > 0)
        {
            // Skip "api" prefix if present
            var startIndex = segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            if (segments.Length > startIndex)
            {
                resourceType = segments[startIndex].ToLowerInvariant();
            }
        }

        // Try to extract resource ID (usually a GUID or integer after the resource type)
        string? resourceId = null;
        for (int i = 1; i < segments.Length; i++)
        {
            if (Guid.TryParse(segments[i], out _) || int.TryParse(segments[i], out _))
            {
                resourceId = segments[i];
                break;
            }
        }

        // Try to extract tenant ID from path
        string? tenantId = null;
        var match = TenantIdRegex.Match(pathValue);
        if (match.Success)
        {
            tenantId = match.Groups[1].Value;
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

        return (resourceType, resourceId, tenantId, description);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers first (for proxied requests)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string? TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}
