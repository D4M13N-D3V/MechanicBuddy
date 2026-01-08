using System;

namespace MechanicBuddy.Management.Api.Domain;

/// <summary>
/// Represents an audit log entry for the Management API.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    // Who made the request
    public int? AdminId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string? AdminRole { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // What action was performed
    public string ActionType { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? TenantId { get; set; }
    public string? ActionDescription { get; set; }

    // When it happened
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int? DurationMs { get; set; }

    // Result
    public int StatusCode { get; set; }
    public bool WasSuccessful { get; set; }
}

/// <summary>
/// Statistics for audit logs.
/// </summary>
public class AuditLogStats
{
    public int TotalRequests { get; set; }
    public int UniqueAdmins { get; set; }
    public int TenantOperations { get; set; }
    public int AuthEvents { get; set; }
    public int FailedRequests { get; set; }
}
