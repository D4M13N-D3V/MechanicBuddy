using MechanicBuddy.Management.Api.Models;

namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Interface for tenant provisioning operations.
/// </summary>
public interface ITenantProvisioningService
{
    /// <summary>
    /// Provisions a new tenant with full infrastructure deployment.
    /// </summary>
    /// <param name="request">Provisioning request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Provisioning result with tenant details.</returns>
    Task<TenantProvisioningResult> ProvisionTenantAsync(
        TenantProvisioningRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deprovisions a tenant and cleans up all resources.
    /// Attempts to determine deployment mode automatically.
    /// </summary>
    /// <param name="tenantId">Tenant ID to deprovision.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deprovisioning was successful.</returns>
    Task<bool> DeprovisionTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deprovisions a tenant with specified deployment mode.
    /// </summary>
    /// <param name="tenantId">Tenant ID to deprovision.</param>
    /// <param name="deploymentMode">"dedicated" for dedicated namespace, "shared" for shared free-tier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deprovisioning was successful.</returns>
    Task<bool> DeprovisionTenantAsync(
        string tenantId,
        string deploymentMode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a tenant's deployment (e.g., scaling, configuration changes).
    /// </summary>
    /// <param name="tenantId">Tenant ID to update.</param>
    /// <param name="request">Update request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated provisioning result.</returns>
    Task<TenantProvisioningResult> UpdateTenantAsync(
        string tenantId,
        TenantProvisioningRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a tenant deployment.
    /// </summary>
    /// <param name="tenantId">Tenant ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current tenant status.</returns>
    Task<TenantStatus> GetTenantStatusAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a tenant provisioning request.
    /// </summary>
    /// <param name="request">Request to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with any errors.</returns>
    Task<ValidationResult> ValidateProvisioningRequestAsync(
        TenantProvisioningRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a unique tenant ID from a company name.
    /// </summary>
    /// <param name="companyName">Company name to convert.</param>
    /// <returns>Unique tenant ID.</returns>
    string GenerateTenantId(string companyName);

    /// <summary>
    /// Suspends a tenant by scaling deployments to 0 replicas.
    /// </summary>
    /// <param name="tenantId">Tenant ID to suspend.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if suspension was successful.</returns>
    Task<bool> SuspendTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a suspended tenant by scaling deployments back up.
    /// </summary>
    /// <param name="tenantId">Tenant ID to resume.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if resumption was successful.</returns>
    Task<bool> ResumeTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Tenant deployment status.
/// </summary>
public class TenantStatus
{
    public string TenantId { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PodStatus> Pods { get; set; } = new();
    public DatabaseStatus? Database { get; set; }
    public string? TenantUrl { get; set; }
    public DateTime? LastChecked { get; set; }
}

/// <summary>
/// Database status information.
/// </summary>
public class DatabaseStatus
{
    public bool IsReady { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Instances { get; set; }
    public int ReadyInstances { get; set; }
}

/// <summary>
/// Validation result for provisioning requests.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
