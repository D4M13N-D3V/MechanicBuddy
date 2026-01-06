using k8s;
using MechanicBuddy.Management.Api.Configuration;

namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Extension methods for registering provisioning services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers tenant provisioning services.
    /// </summary>
    public static IServiceCollection AddTenantProvisioning(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register configuration
        services.Configure<ProvisioningOptions>(
            configuration.GetSection("Provisioning"));

        // Register Kubernetes client
        services.AddSingleton<IKubernetes>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<KubernetesClientFactory>>();

            try
            {
                // Try to load in-cluster configuration first
                var config = KubernetesClientConfiguration.InClusterConfig();
                logger.LogInformation("Using in-cluster Kubernetes configuration");
                return new Kubernetes(config);
            }
            catch (KubeConfigException)
            {
                try
                {
                    // Fall back to kubeconfig file
                    var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
                    logger.LogInformation("Using kubeconfig file for Kubernetes configuration");
                    return new Kubernetes(config);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create Kubernetes client configuration");
                    throw;
                }
            }
        });

        // Register services
        services.AddScoped<IKubernetesClientService, KubernetesClientService>();
        services.AddScoped<IHelmService, HelmService>();
        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();

        return services;
    }
}

/// <summary>
/// Factory class for Kubernetes client logging.
/// </summary>
internal class KubernetesClientFactory
{
}
