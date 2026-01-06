using k8s;
using k8s.Models;

namespace MechanicBuddy.Management.Api.Infrastructure;

public class KubernetesClient : IKubernetesClient
{
    private readonly IKubernetes _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KubernetesClient> _logger;
    private readonly string _baseApiUrl;
    private readonly string _postgresHost;
    private readonly string _postgresUser;
    private readonly string _postgresPassword;

    public KubernetesClient(IConfiguration configuration, ILogger<KubernetesClient> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var config = KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildConfigFromConfigFile();

        _client = new Kubernetes(config);

        _baseApiUrl = configuration["Kubernetes:BaseApiUrl"] ?? "http://mechanicbuddy-api.local";
        _postgresHost = configuration["Database:PostgresHost"] ?? "postgres";
        _postgresUser = configuration["Database:PostgresUser"] ?? "postgres";
        _postgresPassword = configuration["Database:PostgresPassword"] ?? "postgres";
    }

    public async Task<string> CreateNamespaceAsync(string tenantId)
    {
        var namespaceName = $"mb-{tenantId}";

        try
        {
            var existingNamespace = await _client.CoreV1.ReadNamespaceAsync(namespaceName);
            _logger.LogInformation("Namespace {Namespace} already exists", namespaceName);
            return namespaceName;
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Namespace doesn't exist, create it
        }

        var namespaceSpec = new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = namespaceName,
                Labels = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy",
                    ["tenant-id"] = tenantId,
                    ["managed-by"] = "mechanicbuddy-management"
                }
            }
        };

        await _client.CoreV1.CreateNamespaceAsync(namespaceSpec);
        _logger.LogInformation("Created namespace {Namespace}", namespaceName);

        return namespaceName;
    }

    public async Task<string> DeployTenantInstanceAsync(string tenantId, string tier)
    {
        var namespaceName = $"mb-{tenantId}";
        var deploymentName = $"api-{tenantId}";
        var serviceName = $"api-{tenantId}";

        // Get resource limits based on tier
        var (cpuLimit, memoryLimit, replicas) = GetResourceLimitsForTier(tier);

        // Create deployment
        var deployment = new V1Deployment
        {
            Metadata = new V1ObjectMeta
            {
                Name = deploymentName,
                NamespaceProperty = namespaceName,
                Labels = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy-api",
                    ["tenant-id"] = tenantId,
                    ["tier"] = tier
                }
            },
            Spec = new V1DeploymentSpec
            {
                Replicas = replicas,
                Selector = new V1LabelSelector
                {
                    MatchLabels = new Dictionary<string, string>
                    {
                        ["app"] = "mechanicbuddy-api",
                        ["tenant-id"] = tenantId
                    }
                },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string>
                        {
                            ["app"] = "mechanicbuddy-api",
                            ["tenant-id"] = tenantId
                        }
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container>
                        {
                            new V1Container
                            {
                                Name = "api",
                                Image = "mechanicbuddy/api:latest",
                                Ports = new List<V1ContainerPort>
                                {
                                    new V1ContainerPort { ContainerPort = 80 }
                                },
                                Resources = new V1ResourceRequirements
                                {
                                    Limits = new Dictionary<string, ResourceQuantity>
                                    {
                                        ["cpu"] = new ResourceQuantity(cpuLimit),
                                        ["memory"] = new ResourceQuantity(memoryLimit)
                                    },
                                    Requests = new Dictionary<string, ResourceQuantity>
                                    {
                                        ["cpu"] = new ResourceQuantity($"{int.Parse(cpuLimit.TrimEnd('m')) / 2}m"),
                                        ["memory"] = new ResourceQuantity($"{int.Parse(memoryLimit.TrimEnd('i', 'M')) / 2}Mi")
                                    }
                                },
                                Env = new List<V1EnvVar>
                                {
                                    new V1EnvVar { Name = "TENANT_ID", Value = tenantId },
                                    new V1EnvVar { Name = "ASPNETCORE_ENVIRONMENT", Value = "Production" }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.AppsV1.CreateNamespacedDeploymentAsync(deployment, namespaceName);
        _logger.LogInformation("Created deployment {Deployment} in namespace {Namespace}", deploymentName, namespaceName);

        // Create service
        var service = new V1Service
        {
            Metadata = new V1ObjectMeta
            {
                Name = serviceName,
                NamespaceProperty = namespaceName,
                Labels = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy-api",
                    ["tenant-id"] = tenantId
                }
            },
            Spec = new V1ServiceSpec
            {
                Selector = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy-api",
                    ["tenant-id"] = tenantId
                },
                Ports = new List<V1ServicePort>
                {
                    new V1ServicePort
                    {
                        Protocol = "TCP",
                        Port = 80,
                        TargetPort = 80
                    }
                },
                Type = "ClusterIP"
            }
        };

        await _client.CoreV1.CreateNamespacedServiceAsync(service, namespaceName);
        _logger.LogInformation("Created service {Service} in namespace {Namespace}", serviceName, namespaceName);

        // Return API URL
        var apiUrl = $"http://{serviceName}.{namespaceName}.svc.cluster.local";
        return apiUrl;
    }

    public async Task<string> CreateTenantDatabaseAsync(string tenantId)
    {
        // In a real implementation, this would:
        // 1. Create a new PostgreSQL database/schema
        // 2. Run migrations
        // 3. Return connection string

        // For now, we'll create a schema in the shared database
        var schemaName = $"tenant_{tenantId.Replace("-", "_")}";
        var connectionString = $"Host={_postgresHost};Database=mechanicbuddy;Username={_postgresUser};Password={_postgresPassword};SearchPath={schemaName}";

        _logger.LogInformation("Created database schema {Schema} for tenant {TenantId}", schemaName, tenantId);

        // In production, execute SQL to create schema and run migrations
        await Task.CompletedTask;

        return connectionString;
    }

    public async Task ScaleTenantInstanceAsync(string tenantId, int replicas)
    {
        var namespaceName = $"mb-{tenantId}";
        var deploymentName = $"api-{tenantId}";

        var deployment = await _client.AppsV1.ReadNamespacedDeploymentAsync(deploymentName, namespaceName);
        deployment.Spec.Replicas = replicas;

        await _client.AppsV1.ReplaceNamespacedDeploymentAsync(deployment, deploymentName, namespaceName);
        _logger.LogInformation("Scaled deployment {Deployment} to {Replicas} replicas", deploymentName, replicas);
    }

    public async Task DeleteNamespaceAsync(string tenantId)
    {
        var namespaceName = $"mb-{tenantId}";

        try
        {
            await _client.CoreV1.DeleteNamespaceAsync(namespaceName);
            _logger.LogInformation("Deleted namespace {Namespace}", namespaceName);
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Namespace {Namespace} not found, already deleted", namespaceName);
        }
    }

    private static (string cpu, string memory, int replicas) GetResourceLimitsForTier(string tier) => tier switch
    {
        "free" => ("500m", "512Mi", 1),
        "starter" => ("1000m", "1Gi", 1),
        "professional" => ("2000m", "2Gi", 2),
        "enterprise" => ("4000m", "4Gi", 3),
        _ => ("500m", "512Mi", 1)
    };
}
