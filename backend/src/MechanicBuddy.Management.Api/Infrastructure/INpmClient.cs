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
}
