namespace MechanicBuddy.Management.Api.Infrastructure;

public interface INpmClient
{
    /// <summary>
    /// Creates a proxy host for a tenant subdomain
    /// </summary>
    Task<bool> CreateProxyHostAsync(string tenantId);

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
