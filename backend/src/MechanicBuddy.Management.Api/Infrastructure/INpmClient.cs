namespace MechanicBuddy.Management.Api.Infrastructure;

public interface INpmClient
{
    /// <summary>
    /// Creates a proxy host for a tenant subdomain using default forward host
    /// </summary>
    Task<bool> CreateProxyHostAsync(string tenantId);

    /// <summary>
    /// Creates a proxy host for a tenant subdomain with custom forward host/port.
    /// Used for shared free-tier instances where traffic routes to a shared deployment.
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    /// <param name="forwardHost">Custom forward host (internal service address)</param>
    /// <param name="forwardPort">Custom forward port</param>
    Task<bool> CreateProxyHostAsync(string tenantId, string forwardHost, int forwardPort);

    /// <summary>
    /// Deletes a proxy host for a tenant subdomain
    /// </summary>
    Task<bool> DeleteProxyHostAsync(string tenantId);

    /// <summary>
    /// Creates a proxy host for a custom domain with Let's Encrypt SSL
    /// </summary>
    Task<bool> CreateCustomDomainProxyHostAsync(string tenantId, string customDomain);

    /// <summary>
    /// Deletes a proxy host for a custom domain
    /// </summary>
    Task<bool> DeleteCustomDomainProxyHostAsync(string customDomain);
}
