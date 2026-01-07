using k8s;
using k8s.Models;

namespace MechanicBuddy.Management.Api.Infrastructure;

public class KubernetesClient : IKubernetesClient
{
    private readonly IKubernetes _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KubernetesClient> _logger;
    private readonly ICloudflareClient _cloudflareClient;
    private readonly INpmClient _npmClient;
    private readonly ITenantDatabaseProvisioner _dbProvisioner;
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
    private readonly string _apiImage;
    private readonly string _frontendImage;
    private readonly string _jwtSecret;
    private readonly string _consumerSecret;

    public KubernetesClient(
        IConfiguration configuration,
        ILogger<KubernetesClient> logger,
        ICloudflareClient cloudflareClient,
        INpmClient npmClient,
        ITenantDatabaseProvisioner dbProvisioner,
        IKubernetes? kubernetes = null)
    {
        _configuration = configuration;
        _logger = logger;
        _cloudflareClient = cloudflareClient;
        _npmClient = npmClient;
        _dbProvisioner = dbProvisioner;

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
        _apiImage = configuration["Kubernetes:ApiImage"] ?? "ghcr.io/d4m13n-d3v/mechanicbuddy-api:latest";
        _frontendImage = configuration["Kubernetes:FrontendImage"] ?? "ghcr.io/d4m13n-d3v/mechanicbuddy-web:latest";
        // Use Jwt:SecretKey (Management API's JWT key) as the base for tenant secrets
        // These will be passed to tenant instances for their JwtOptions configuration
        _jwtSecret = configuration["Jwt:SecretKey"]
            ?? configuration["TenantSecrets:JwtSecret"]
            ?? throw new InvalidOperationException("Jwt:SecretKey or TenantSecrets:JwtSecret is required");
        _consumerSecret = configuration["TenantSecrets:ConsumerSecret"]
            ?? configuration["Jwt:SecretKey"]  // Fallback to same key if not separately configured
            ?? throw new InvalidOperationException("TenantSecrets:ConsumerSecret or Jwt:SecretKey is required");
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
        var apiDeploymentName = $"api-{tenantId}";
        var apiServiceName = $"api-{tenantId}";
        var frontendDeploymentName = $"frontend-{tenantId}";
        var frontendServiceName = $"frontend-{tenantId}";
        var apiSecretName = $"api-secrets-{tenantId}";
        var frontendSecretName = $"frontend-secrets-{tenantId}";
        var schemaName = $"tenant_{tenantId.Replace("-", "_")}";
        var imagePullSecretName = "ghcr-credentials";

        // Get resource limits based on tier
        var (cpuLimit, memoryLimit, replicas) = GetResourceLimitsForTier(tier);

        // Copy GHCR credentials secret to tenant namespace
        await CopyImagePullSecretAsync(namespaceName, imagePullSecretName);

        // Create appsettings.Secrets.json content for tenant
        var secretsJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            PdfDirectory = "/var/mechanicbuddy/pdf",
            PuppeteerPath = "/opt/puppeteer",
            JwtOptions = new
            {
                Secret = _jwtSecret,
                ConsumerSecret = _consumerSecret
            },
            DbOptions = new
            {
                Host = _postgresHost,
                Port = 5432,
                UserId = _postgresUser,
                Password = _postgresPassword,
                Name = "mechanicbuddy",
                Schema = schemaName,
                MultiTenancy = new { Enabled = true, TenantId = tenantId }
            },
            SmtpOptions = new
            {
                Host = _resendSmtpHost,
                Port = _resendSmtpPort.ToString(),
                User = _resendSmtpUser,
                Password = _resendSmtpPassword
            },
            Cors = new
            {
                Mode = "open",
                AppHost = $"{tenantId}.{_baseDomain}"
            }
        }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        // Create API secret
        var apiSecret = new V1Secret
        {
            Metadata = new V1ObjectMeta
            {
                Name = apiSecretName,
                NamespaceProperty = namespaceName,
                Labels = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy-api",
                    ["tenant-id"] = tenantId
                }
            },
            Type = "Opaque",
            StringData = new Dictionary<string, string>
            {
                ["appsettings.Secrets.json"] = secretsJson
            }
        };

        try
        {
            await _client.CoreV1.CreateNamespacedSecretAsync(apiSecret, namespaceName);
            _logger.LogInformation("Created API secret {Secret} in namespace {Namespace}", apiSecretName, namespaceName);
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            await _client.CoreV1.ReplaceNamespacedSecretAsync(apiSecret, apiSecretName, namespaceName);
            _logger.LogInformation("Updated existing API secret {Secret} in namespace {Namespace}", apiSecretName, namespaceName);
        }

        // Create Frontend secret (environment variables)
        var tenantDomain = $"{tenantId}.{_baseDomain}";
        var frontendSecret = new V1Secret
        {
            Metadata = new V1ObjectMeta
            {
                Name = frontendSecretName,
                NamespaceProperty = namespaceName,
                Labels = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy-frontend",
                    ["tenant-id"] = tenantId
                }
            },
            Type = "Opaque",
            StringData = new Dictionary<string, string>
            {
                ["SERVER_SECRET"] = _consumerSecret,
                ["SESSION_SECRET"] = Convert.ToBase64String([.. Guid.NewGuid().ToByteArray(), .. Guid.NewGuid().ToByteArray()]),
                ["API_URL"] = $"http://{apiServiceName}:80",
                ["NEXT_PUBLIC_API_URL"] = $"https://{tenantDomain}/api",
                ["NEXT_PUBLIC_SESSION_TIMEOUT"] = "3600"
            }
        };

        try
        {
            await _client.CoreV1.CreateNamespacedSecretAsync(frontendSecret, namespaceName);
            _logger.LogInformation("Created Frontend secret {Secret} in namespace {Namespace}", frontendSecretName, namespaceName);
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            await _client.CoreV1.ReplaceNamespacedSecretAsync(frontendSecret, frontendSecretName, namespaceName);
            _logger.LogInformation("Updated existing Frontend secret {Secret} in namespace {Namespace}", frontendSecretName, namespaceName);
        }

        // Create API deployment
        var apiDeployment = new V1Deployment
        {
            Metadata = new V1ObjectMeta
            {
                Name = apiDeploymentName,
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
                        ImagePullSecrets = new List<V1LocalObjectReference>
                        {
                            new V1LocalObjectReference { Name = imagePullSecretName }
                        },
                        Volumes = new List<V1Volume>
                        {
                            new V1Volume
                            {
                                Name = "secrets-volume",
                                Secret = new V1SecretVolumeSource
                                {
                                    SecretName = apiSecretName
                                }
                            }
                        },
                        Containers = new List<V1Container>
                        {
                            new V1Container
                            {
                                Name = "api",
                                Image = _apiImage,
                                Ports = new List<V1ContainerPort>
                                {
                                    new V1ContainerPort { ContainerPort = 15567 }
                                },
                                VolumeMounts = new List<V1VolumeMount>
                                {
                                    new V1VolumeMount
                                    {
                                        Name = "secrets-volume",
                                        MountPath = "/app/appsettings.Secrets.json",
                                        SubPath = "appsettings.Secrets.json",
                                        ReadOnlyProperty = true
                                    }
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

        await _client.AppsV1.CreateNamespacedDeploymentAsync(apiDeployment, namespaceName);
        _logger.LogInformation("Created API deployment {Deployment} in namespace {Namespace}", apiDeploymentName, namespaceName);

        // Create Frontend deployment
        var frontendDeployment = new V1Deployment
        {
            Metadata = new V1ObjectMeta
            {
                Name = frontendDeploymentName,
                NamespaceProperty = namespaceName,
                Labels = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy-frontend",
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
                        ["app"] = "mechanicbuddy-frontend",
                        ["tenant-id"] = tenantId
                    }
                },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string>
                        {
                            ["app"] = "mechanicbuddy-frontend",
                            ["tenant-id"] = tenantId
                        }
                    },
                    Spec = new V1PodSpec
                    {
                        ImagePullSecrets = new List<V1LocalObjectReference>
                        {
                            new V1LocalObjectReference { Name = imagePullSecretName }
                        },
                        Containers = new List<V1Container>
                        {
                            new V1Container
                            {
                                Name = "frontend",
                                Image = _frontendImage,
                                Ports = new List<V1ContainerPort>
                                {
                                    new V1ContainerPort { ContainerPort = 3000 }
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
                                EnvFrom = new List<V1EnvFromSource>
                                {
                                    new V1EnvFromSource
                                    {
                                        SecretRef = new V1SecretEnvSource
                                        {
                                            Name = frontendSecretName
                                        }
                                    }
                                },
                                Env = new List<V1EnvVar>
                                {
                                    new V1EnvVar { Name = "TENANT_ID", Value = tenantId },
                                    new V1EnvVar { Name = "NODE_ENV", Value = "production" }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.AppsV1.CreateNamespacedDeploymentAsync(frontendDeployment, namespaceName);
        _logger.LogInformation("Created Frontend deployment {Deployment} in namespace {Namespace}", frontendDeploymentName, namespaceName);

        // Create API service
        var apiService = new V1Service
        {
            Metadata = new V1ObjectMeta
            {
                Name = apiServiceName,
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
                        TargetPort = 15567
                    }
                },
                Type = "ClusterIP"
            }
        };

        await _client.CoreV1.CreateNamespacedServiceAsync(apiService, namespaceName);
        _logger.LogInformation("Created API service {Service} in namespace {Namespace}", apiServiceName, namespaceName);

        // Create Frontend service
        var frontendService = new V1Service
        {
            Metadata = new V1ObjectMeta
            {
                Name = frontendServiceName,
                NamespaceProperty = namespaceName,
                Labels = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy-frontend",
                    ["tenant-id"] = tenantId
                }
            },
            Spec = new V1ServiceSpec
            {
                Selector = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy-frontend",
                    ["tenant-id"] = tenantId
                },
                Ports = new List<V1ServicePort>
                {
                    new V1ServicePort
                    {
                        Protocol = "TCP",
                        Port = 80,
                        TargetPort = 3000
                    }
                },
                Type = "ClusterIP"
            }
        };

        await _client.CoreV1.CreateNamespacedServiceAsync(frontendService, namespaceName);
        _logger.LogInformation("Created Frontend service {Service} in namespace {Namespace}", frontendServiceName, namespaceName);

        // Copy wildcard TLS secret from cert-manager namespace to tenant namespace
        var tlsSecretName = $"tls-wildcard-{_baseDomain.Replace(".", "-")}";
        await CopyWildcardSecretAsync(namespaceName, tlsSecretName);

        // Create Ingress for external access with path-based routing
        var ingressName = $"ingress-{tenantId}";

        var ingress = new V1Ingress
        {
            Metadata = new V1ObjectMeta
            {
                Name = ingressName,
                NamespaceProperty = namespaceName,
                Labels = new Dictionary<string, string>
                {
                    ["app"] = "mechanicbuddy",
                    ["tenant-id"] = tenantId
                },
                Annotations = new Dictionary<string, string>
                {
                    // No cert-manager annotation needed - using wildcard cert
                    ["nginx.ingress.kubernetes.io/proxy-body-size"] = "50m",
                    ["nginx.ingress.kubernetes.io/proxy-read-timeout"] = "300",
                    ["nginx.ingress.kubernetes.io/proxy-send-timeout"] = "300",
                    // Disable SSL redirect since Cloudflare/NPM handles SSL termination
                    ["nginx.ingress.kubernetes.io/ssl-redirect"] = "false",
                    // Rewrite /api/* to /* for the API backend
                    ["nginx.ingress.kubernetes.io/use-regex"] = "true"
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
                                // API routes - /api/* goes to API service
                                new V1HTTPIngressPath
                                {
                                    Path = "/api",
                                    PathType = "Prefix",
                                    Backend = new V1IngressBackend
                                    {
                                        Service = new V1IngressServiceBackend
                                        {
                                            Name = apiServiceName,
                                            Port = new V1ServiceBackendPort { Number = 80 }
                                        }
                                    }
                                },
                                // All other routes go to Frontend
                                new V1HTTPIngressPath
                                {
                                    Path = "/",
                                    PathType = "Prefix",
                                    Backend = new V1IngressBackend
                                    {
                                        Service = new V1IngressServiceBackend
                                        {
                                            Name = frontendServiceName,
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

        // Create proxy host in Nginx Proxy Manager
        var proxyCreated = await _npmClient.CreateProxyHostAsync(tenantId);
        if (!proxyCreated)
        {
            _logger.LogWarning("Failed to create NPM proxy host for {Domain}", tenantDomain);
        }

        // Return external URL
        var apiUrl = $"https://{tenantDomain}";
        return apiUrl;
    }

    public async Task<string> CreateTenantDatabaseAsync(string tenantId)
    {
        _logger.LogInformation("Creating database for tenant {TenantId}", tenantId);

        // Use the database provisioner to create schema and run migrations
        var connectionString = await _dbProvisioner.ProvisionTenantDatabaseAsync(tenantId);

        _logger.LogInformation("Successfully created database for tenant {TenantId}", tenantId);

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

        // Delete proxy host from Nginx Proxy Manager
        var proxyDeleted = await _npmClient.DeleteProxyHostAsync(tenantId);
        if (!proxyDeleted)
        {
            _logger.LogWarning("Failed to delete NPM proxy host for tenant {TenantId}", tenantId);
        }
    }

    /// <summary>
    /// Copies the GHCR image pull secret from mechanicbuddy-system namespace to the target namespace
    /// </summary>
    private async Task CopyImagePullSecretAsync(string targetNamespace, string secretName)
    {
        const string sourceNamespace = "mechanicbuddy-system";

        try
        {
            // Check if secret already exists in target namespace
            try
            {
                await _client.CoreV1.ReadNamespacedSecretAsync(secretName, targetNamespace);
                _logger.LogInformation("Image pull secret {SecretName} already exists in namespace {Namespace}", secretName, targetNamespace);
                return;
            }
            catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Secret doesn't exist, continue to copy
            }

            // Read the secret from mechanicbuddy-system namespace
            var sourceSecret = await _client.CoreV1.ReadNamespacedSecretAsync(secretName, sourceNamespace);

            // Create a copy in the target namespace
            var targetSecret = new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = secretName,
                    NamespaceProperty = targetNamespace,
                    Labels = new Dictionary<string, string>
                    {
                        ["app"] = "mechanicbuddy",
                        ["copied-from-namespace"] = sourceNamespace,
                        ["copied-from-secret"] = secretName
                    }
                },
                Type = sourceSecret.Type,
                Data = sourceSecret.Data
            };

            await _client.CoreV1.CreateNamespacedSecretAsync(targetSecret, targetNamespace);
            _logger.LogInformation("Copied image pull secret {SecretName} to namespace {Namespace}", secretName, targetNamespace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy image pull secret {SecretName} to namespace {Namespace}", secretName, targetNamespace);
            throw;
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
