namespace MechanicBuddy.Management.Api.Configuration;

/// <summary>
/// Configuration options for tenant provisioning.
/// </summary>
public class ProvisioningOptions
{
    /// <summary>
    /// Path to the Helm chart directory.
    /// </summary>
    public string HelmChartPath { get; set; } = "/app/infrastructure/helm/charts/mechanicbuddy-tenant";

    /// <summary>
    /// Base domain for tenant subdomains (e.g., mechanicbuddy.app).
    /// </summary>
    public string BaseDomain { get; set; } = "mechanicbuddy.app";

    /// <summary>
    /// Kubernetes namespace prefix for tenants.
    /// </summary>
    public string NamespacePrefix { get; set; } = "tenant-";

    /// <summary>
    /// Default cluster issuer for TLS certificates.
    /// </summary>
    public string ClusterIssuer { get; set; } = "letsencrypt-prod";

    /// <summary>
    /// Timeout for provisioning operations (in seconds).
    /// </summary>
    public int ProvisioningTimeoutSeconds { get; set; } = 600;

    /// <summary>
    /// Timeout for waiting for pods to be ready (in seconds).
    /// </summary>
    public int PodReadyTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Timeout for database migrations (in seconds).
    /// </summary>
    public int MigrationTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Resource limits per subscription tier.
    /// </summary>
    public Dictionary<string, TierResourceLimits> TierLimits { get; set; } = new()
    {
        ["demo"] = new TierResourceLimits
        {
            PostgresInstances = 1,
            PostgresStorageSize = "5Gi",
            PostgresMemoryRequest = "128Mi",
            PostgresMemoryLimit = "256Mi",
            PostgresCpuRequest = "50m",
            PostgresCpuLimit = "250m",
            ApiReplicas = 1,
            ApiMemoryRequest = "128Mi",
            ApiMemoryLimit = "256Mi",
            ApiCpuRequest = "50m",
            ApiCpuLimit = "250m",
            WebReplicas = 1,
            WebMemoryRequest = "64Mi",
            WebMemoryLimit = "128Mi",
            WebCpuRequest = "25m",
            WebCpuLimit = "100m",
            MechanicLimit = 2,
            ExpirationDays = 7,
            BackupEnabled = false
        },
        ["free"] = new TierResourceLimits
        {
            PostgresInstances = 1,
            PostgresStorageSize = "10Gi",
            PostgresMemoryRequest = "256Mi",
            PostgresMemoryLimit = "512Mi",
            PostgresCpuRequest = "100m",
            PostgresCpuLimit = "500m",
            ApiReplicas = 1,
            ApiMemoryRequest = "256Mi",
            ApiMemoryLimit = "512Mi",
            ApiCpuRequest = "100m",
            ApiCpuLimit = "500m",
            WebReplicas = 1,
            WebMemoryRequest = "128Mi",
            WebMemoryLimit = "256Mi",
            WebCpuRequest = "50m",
            WebCpuLimit = "250m",
            MechanicLimit = 5,
            ExpirationDays = null,
            BackupEnabled = false
        },
        ["professional"] = new TierResourceLimits
        {
            PostgresInstances = 1,
            PostgresStorageSize = "50Gi",
            PostgresMemoryRequest = "512Mi",
            PostgresMemoryLimit = "1Gi",
            PostgresCpuRequest = "250m",
            PostgresCpuLimit = "1000m",
            ApiReplicas = 2,
            ApiMemoryRequest = "512Mi",
            ApiMemoryLimit = "1Gi",
            ApiCpuRequest = "250m",
            ApiCpuLimit = "1000m",
            WebReplicas = 2,
            WebMemoryRequest = "256Mi",
            WebMemoryLimit = "512Mi",
            WebCpuRequest = "100m",
            WebCpuLimit = "500m",
            MechanicLimit = 20,
            ExpirationDays = null,
            BackupEnabled = true
        },
        ["enterprise"] = new TierResourceLimits
        {
            PostgresInstances = 3,
            PostgresStorageSize = "200Gi",
            PostgresMemoryRequest = "1Gi",
            PostgresMemoryLimit = "2Gi",
            PostgresCpuRequest = "500m",
            PostgresCpuLimit = "2000m",
            ApiReplicas = 3,
            ApiMemoryRequest = "1Gi",
            ApiMemoryLimit = "2Gi",
            ApiCpuRequest = "500m",
            ApiCpuLimit = "2000m",
            WebReplicas = 3,
            WebMemoryRequest = "512Mi",
            WebMemoryLimit = "1Gi",
            WebCpuRequest = "250m",
            WebCpuLimit = "1000m",
            MechanicLimit = null, // Unlimited
            ExpirationDays = null,
            BackupEnabled = true
        }
    };

    /// <summary>
    /// Default admin credentials for new tenants.
    /// </summary>
    public AdminCredentials DefaultAdmin { get; set; } = new()
    {
        Username = "admin",
        Password = "ChangeMeOnFirstLogin!"
    };

    /// <summary>
    /// Storage class for PostgreSQL persistent volumes.
    /// </summary>
    public string StorageClass { get; set; } = "local-path";

    /// <summary>
    /// Container registry configuration.
    /// </summary>
    public ContainerRegistry Registry { get; set; } = new()
    {
        ApiRepository = "ghcr.io/mechanicbuddy/api",
        WebRepository = "ghcr.io/mechanicbuddy/web",
        DbUpRepository = "ghcr.io/mechanicbuddy/dbup",
        DefaultTag = "latest",
        PullPolicy = "IfNotPresent"
    };
}

/// <summary>
/// Resource limits for a subscription tier.
/// </summary>
public class TierResourceLimits
{
    public int PostgresInstances { get; set; }
    public string PostgresStorageSize { get; set; } = string.Empty;
    public string PostgresMemoryRequest { get; set; } = string.Empty;
    public string PostgresMemoryLimit { get; set; } = string.Empty;
    public string PostgresCpuRequest { get; set; } = string.Empty;
    public string PostgresCpuLimit { get; set; } = string.Empty;
    public int ApiReplicas { get; set; }
    public string ApiMemoryRequest { get; set; } = string.Empty;
    public string ApiMemoryLimit { get; set; } = string.Empty;
    public string ApiCpuRequest { get; set; } = string.Empty;
    public string ApiCpuLimit { get; set; } = string.Empty;
    public int WebReplicas { get; set; }
    public string WebMemoryRequest { get; set; } = string.Empty;
    public string WebMemoryLimit { get; set; } = string.Empty;
    public string WebCpuRequest { get; set; } = string.Empty;
    public string WebCpuLimit { get; set; } = string.Empty;
    public int? MechanicLimit { get; set; }
    public int? ExpirationDays { get; set; }
    public bool BackupEnabled { get; set; }
    public string? StorageClass { get; set; }
}

/// <summary>
/// Default admin credentials configuration.
/// </summary>
public class AdminCredentials
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Container registry configuration.
/// </summary>
public class ContainerRegistry
{
    public string ApiRepository { get; set; } = string.Empty;
    public string WebRepository { get; set; } = string.Empty;
    public string DbUpRepository { get; set; } = string.Empty;
    public string DefaultTag { get; set; } = string.Empty;
    public string PullPolicy { get; set; } = string.Empty;
}
