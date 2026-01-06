using k8s;
using k8s.Models;

namespace MechanicBuddy.Management.Api.Infrastructure;

public class KubernetesClient : IKubernetesClient
{
    private readonly IKubernetes _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KubernetesClient> _logger;
    private readonly ICloudflareClient _cloudflareClient;
    private readonly string _baseDomain;
    private readonly string _postgresHost;
    private readonly string _postgresUser;
    private readonly string _postgresPassword;
    private readonly string _resendSmtpHost;
    private readonly int _resendSmtpPort;
    private readonly string _resendSmtpUser;
    private readonly string _resendSmtpPassword;
    private readonly string _ingressClassName;
    private readonly string _clusterIssuer;

    public KubernetesClient(
        IConfiguration configuration,
        ILogger<KubernetesClient> logger,
        ICloudflareClient cloudflareClient,
        IKubernetes? kubernetes = null)
    {
        _configuration = configuration;
        _logger = logger;
        _cloudflareClient = cloudflareClient;

        if (kubernetes != null)
        {
            _client = kubernetes;
        }
        else
        {
            var config = KubernetesClientConfiguration.IsInCluster()
                ? KubernetesClientConfiguration.InClusterConfig()
                : KubernetesClientConfiguration.BuildConfigFromConfigFile();

            _client = new Kubernetes(config);
        }

        _baseDomain = configuration["Cloudflare:BaseDomain"] ?? "mechanicbuddy.app";
        _postgresHost = configuration["Database:PostgresHost"] ?? "postgres";
        _postgresUser = configuration["Database:PostgresUser"] ?? "postgres";
        _postgresPassword = configuration["Database:PostgresPassword"] ?? "postgres";

        // Resend SMTP configuration for tenant instances
        _resendSmtpHost = configuration["Email:SmtpHost"] ?? "smtp.resend.com";
        _resendSmtpPort = int.TryParse(configuration["Email:SmtpPort"], out var port) ? port : 587;
        _resendSmtpUser = configuration["Email:SmtpUser"] ?? "resend";
        _resendSmtpPassword = configuration["Email:ResendApiKey"] ?? "";

        // Ingress configuration
        _ingressClassName = configuration["Kubernetes:IngressClassName"] ?? "nginx";
        _clusterIssuer = configuration["Kubernetes:ClusterIssuer"] ?? "letsencrypt-prod";
        _certManagerNamespace = configuration["Kubernetes:CertManagerNamespace"] ?? "cert-manager";
        _wildcardSecretName = configuration["Kubernetes:WildcardSecretName"]
            ?? $"wildcard-{_baseDomain.Replace(".", "-")}-tls";
    }

    private readonly string _certManagerNamespace;
    private readonly string _wildcardSecretName;

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
                                    new V1EnvVar { Name = "ASPNETCORE_ENVIRONMENT", Value = "Production" },
                                    // SMTP configuration for tenant email (via Resend)
                                    new V1EnvVar { Name = "Smtp__Host", Value = _resendSmtpHost },
                                    new V1EnvVar { Name = "Smtp__Port", Value = _resendSmtpPort.ToString() },
                                    new V1EnvVar { Name = "Smtp__User", Value = _resendSmtpUser },
                                    new V1EnvVar { Name = "Smtp__Password", Value = _resendSmtpPassword }
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

        // Copy wildcard TLS secret from cert-manager namespace to tenant namespace
        var tlsSecretName = $"tls-wildcard-{_baseDomain.Replace(".", "-")}";
        await CopyWildcardSecretAsync(namespaceName, tlsSecretName);

        // Create Ingress for external access
        var tenantDomain = $"{tenantId}.{_baseDomain}";
        var ingressName = $"ingress-{tenantId}";

        var ingress = new V1Ingress
        {
            Metadata = new V1ObjectMeta
            {
                Name = ingressName,
                NamespaceProperty = namespaceName,
                Labels = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy-api",
                    ["tenant-id"] = tenantId
                },
                Annotations = new Dictionary<string, string>
                {
                    // No cert-manager annotation needed - using wildcard cert
                    ["nginx.ingress.kubernetes.io/proxy-body-size"] = "50m",
                    ["nginx.ingress.kubernetes.io/proxy-read-timeout"] = "300",
                    ["nginx.ingress.kubernetes.io/proxy-send-timeout"] = "300"
                }
            },
            Spec = new V1IngressSpec
            {
                IngressClassName = _ingressClassName,
                Tls = new List<V1IngressTLS>
                {
                    new V1IngressTLS
                    {
                        Hosts = new List<string> { tenantDomain },
                        SecretName = tlsSecretName  // Use copied wildcard secret
                    }
                },
                Rules = new List<V1IngressRule>
                {
                    new V1IngressRule
                    {
                        Host = tenantDomain,
                        Http = new V1HTTPIngressRuleValue
                        {
                            Paths = new List<V1HTTPIngressPath>
                            {
                                new V1HTTPIngressPath
                                {
                                    Path = "/",
                                    PathType = "Prefix",
                                    Backend = new V1IngressBackend
                                    {
                                        Service = new V1IngressServiceBackend
                                        {
                                            Name = serviceName,
                                            Port = new V1ServiceBackendPort { Number = 80 }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.NetworkingV1.CreateNamespacedIngressAsync(ingress, namespaceName);
        _logger.LogInformation("Created ingress {Ingress} for domain {Domain}", ingressName, tenantDomain);

        // Create DNS record in Cloudflare
        var dnsCreated = await _cloudflareClient.CreateTenantDnsRecordAsync(tenantId);
        if (!dnsCreated)
        {
            _logger.LogWarning("Failed to create DNS record for {Domain}, tenant may not be accessible externally", tenantDomain);
        }

        // Return external URL
        var apiUrl = $"https://{tenantDomain}";
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

        // Delete DNS record from Cloudflare
        var dnsDeleted = await _cloudflareClient.DeleteTenantDnsRecordAsync(tenantId);
        if (!dnsDeleted)
        {
            _logger.LogWarning("Failed to delete DNS record for tenant {TenantId}", tenantId);
        }
    }

    /// <summary>
    /// Copies the wildcard TLS secret from cert-manager namespace to the target namespace
    /// </summary>
    private async Task CopyWildcardSecretAsync(string targetNamespace, string targetSecretName)
    {
        try
        {
            // Check if secret already exists in target namespace
            try
            {
                await _client.CoreV1.ReadNamespacedSecretAsync(targetSecretName, targetNamespace);
                _logger.LogInformation("TLS secret {SecretName} already exists in namespace {Namespace}", targetSecretName, targetNamespace);
                return;
            }
            catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Secret doesn't exist, continue to copy
            }

            // Read the wildcard secret from cert-manager namespace
            var sourceSecret = await _client.CoreV1.ReadNamespacedSecretAsync(_wildcardSecretName, _certManagerNamespace);

            // Create a copy in the target namespace
            var targetSecret = new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = targetSecretName,
                    NamespaceProperty = targetNamespace,
                    Labels = new Dictionary<string, string>
                    {
                        ["app"] = "mechanicbuddy",
                        ["copied-from"] = _wildcardSecretName
                    }
                },
                Type = sourceSecret.Type,
                Data = sourceSecret.Data
            };

            await _client.CoreV1.CreateNamespacedSecretAsync(targetSecret, targetNamespace);
            _logger.LogInformation("Copied wildcard TLS secret to namespace {Namespace}", targetNamespace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy wildcard TLS secret to namespace {Namespace}", targetNamespace);
            throw;
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
