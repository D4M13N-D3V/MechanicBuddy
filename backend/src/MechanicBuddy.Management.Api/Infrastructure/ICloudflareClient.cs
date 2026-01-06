namespace MechanicBuddy.Management.Api.Infrastructure;

public interface ICloudflareClient
{
    /// <summary>
    /// Creates a DNS CNAME record for a tenant subdomain
    /// </summary>
    /// <param name="subdomain">The subdomain (e.g., "acme-auto" for acme-auto.mechanicbuddy.app)</param>
    /// <param name="target">The target for the CNAME (typically the ingress controller)</param>
    /// <returns>True if successful</returns>
    Task<bool> CreateTenantDnsRecordAsync(string subdomain, string? target = null);

    /// <summary>
    /// Deletes a DNS record for a tenant subdomain
    /// </summary>
    Task<bool> DeleteTenantDnsRecordAsync(string subdomain);

    /// <summary>
    /// Checks if a DNS record exists for a subdomain
    /// </summary>
    Task<bool> DnsRecordExistsAsync(string subdomain);
}
