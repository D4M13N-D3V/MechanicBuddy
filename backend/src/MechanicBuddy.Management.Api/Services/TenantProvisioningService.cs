using MechanicBuddy.Management.Api.Configuration;
using MechanicBuddy.Management.Api.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Service for provisioning and managing tenant deployments.
/// </summary>
public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly IKubernetesClientService _k8sClient;
    private readonly IHelmService _helmService;
    private readonly ILogger<TenantProvisioningService> _logger;
    private readonly ProvisioningOptions _options;

    public TenantProvisioningService(
        IKubernetesClientService k8sClient,
        IHelmService helmService,
        ILogger<TenantProvisioningService> logger,
        IOptions<ProvisioningOptions> options)
    {
        _k8sClient = k8sClient;
        _helmService = helmService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<TenantProvisioningResult> ProvisionTenantAsync(
        TenantProvisioningRequest request,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var result = new TenantProvisioningResult
        {
            ProvisionedAt = startTime,
            SubscriptionTier = request.SubscriptionTier
        };

        try
        {
            // Step 1: Validate request
            AddLog(result, "Info", "ValidateRequest", "Validating provisioning request");
            var validation = await ValidateProvisioningRequestAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.ErrorMessage = $"Validation failed: {string.Join(", ", validation.Errors)}";
                return result;
            }

            // Step 2: Generate or use provided tenant ID
            var tenantId = string.IsNullOrEmpty(request.TenantId)
                ? GenerateTenantId(request.CompanyName)
                : request.TenantId;

            result.TenantId = tenantId;
            result.Namespace = $"{_options.NamespacePrefix}{tenantId}";
            result.HelmRelease = $"tenant-{tenantId}";

            AddLog(result, "Info", "GenerateTenantId", $"Generated tenant ID: {tenantId}");

            // Step 3: Check if namespace already exists
            var namespaceExists = await _k8sClient.NamespaceExistsAsync(result.Namespace, cancellationToken);
            if (namespaceExists)
            {
                result.Success = false;
                result.ErrorMessage = $"Tenant with ID '{tenantId}' already exists";
                AddLog(result, "Error", "CheckNamespace", result.ErrorMessage);
                return result;
            }

            // Step 4: Build Helm values
            AddLog(result, "Info", "BuildHelmValues", "Building Helm chart values");
            var helmValues = BuildHelmValues(request, tenantId);

            // Step 5: Deploy Helm chart
            AddLog(result, "Info", "DeployHelm", $"Deploying Helm chart to namespace {result.Namespace}");
            var (helmSuccess, helmOutput) = await _helmService.InstallAsync(
                result.HelmRelease,
                _options.HelmChartPath,
                result.Namespace,
                helmValues,
                createNamespace: true,
                timeout: _options.ProvisioningTimeoutSeconds,
                cancellationToken: cancellationToken);

            if (!helmSuccess)
            {
                result.Success = false;
                result.ErrorMessage = $"Helm installation failed: {helmOutput}";
                AddLog(result, "Error", "DeployHelm", result.ErrorMessage);
                return result;
            }

            AddLog(result, "Info", "DeployHelm", "Helm chart deployed successfully");

            // Step 6: Wait for PostgreSQL cluster to be ready
            AddLog(result, "Info", "WaitForDatabase", "Waiting for PostgreSQL cluster to be ready");
            var dbReady = await WaitForPostgresClusterAsync(
                result.Namespace,
                _options.PodReadyTimeoutSeconds,
                cancellationToken);

            if (!dbReady)
            {
                result.Success = false;
                result.ErrorMessage = "PostgreSQL cluster failed to become ready";
                AddLog(result, "Error", "WaitForDatabase", result.ErrorMessage);
                return result;
            }

            AddLog(result, "Info", "WaitForDatabase", "PostgreSQL cluster is ready");

            // Step 7: Wait for API pod to be ready
            AddLog(result, "Info", "WaitForAPI", "Waiting for API service to be ready");
            var apiReady = await _k8sClient.WaitForPodsReadyAsync(
                result.Namespace,
                labelSelector: "app.kubernetes.io/component=api",
                timeoutSeconds: _options.PodReadyTimeoutSeconds,
                cancellationToken: cancellationToken);

            if (!apiReady)
            {
                result.Success = false;
                result.ErrorMessage = "API service failed to become ready";
                AddLog(result, "Error", "WaitForAPI", result.ErrorMessage);
                return result;
            }

            AddLog(result, "Info", "WaitForAPI", "API service is ready");

            // Step 8: Wait for Web pod to be ready
            AddLog(result, "Info", "WaitForWeb", "Waiting for Web frontend to be ready");
            var webReady = await _k8sClient.WaitForPodsReadyAsync(
                result.Namespace,
                labelSelector: "app.kubernetes.io/component=web",
                timeoutSeconds: _options.PodReadyTimeoutSeconds,
                cancellationToken: cancellationToken);

            if (!webReady)
            {
                AddLog(result, "Warning", "WaitForWeb", "Web frontend failed to become ready (non-critical)");
            }
            else
            {
                AddLog(result, "Info", "WaitForWeb", "Web frontend is ready");
            }

            // Step 9: Set tenant URLs
            var domain = string.IsNullOrEmpty(request.CustomDomain)
                ? $"{tenantId}.{_options.BaseDomain}"
                : request.CustomDomain;

            result.TenantUrl = $"https://{domain}";
            result.ApiUrl = $"https://{domain}/api";

            // Step 10: Set admin credentials
            result.AdminUsername = _options.DefaultAdmin.Username;
            result.AdminPassword = _options.DefaultAdmin.Password;

            // Step 11: Set resource allocation
            var tierLimits = GetTierLimits(request.SubscriptionTier, request.ResourceOverrides);
            result.Resources = new ResourceAllocation
            {
                PostgresInstances = tierLimits.PostgresInstances,
                PostgresStorage = tierLimits.PostgresStorageSize,
                ApiReplicas = tierLimits.ApiReplicas,
                WebReplicas = tierLimits.WebReplicas,
                MechanicLimit = tierLimits.MechanicLimit,
                BackupEnabled = tierLimits.BackupEnabled
            };

            // Step 12: Set expiration if demo/trial
            if (request.SubscriptionTier == "demo" && tierLimits.ExpirationDays.HasValue)
            {
                result.ExpiresAt = DateTime.UtcNow.AddDays(tierLimits.ExpirationDays.Value);
            }

            // Step 13: Set Stripe information
            result.StripeCustomerId = request.StripeCustomerId;

            // Success!
            result.Success = true;
            result.ProvisioningDuration = DateTime.UtcNow - startTime;
            AddLog(result, "Info", "Complete", $"Tenant provisioned successfully in {result.ProvisioningDuration.TotalSeconds:F1}s");

            _logger.LogInformation("Successfully provisioned tenant {TenantId} in {Duration}s",
                tenantId, result.ProvisioningDuration.TotalSeconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision tenant");
            result.Success = false;
            result.ErrorMessage = $"Unexpected error: {ex.Message}";
            result.ProvisioningDuration = DateTime.UtcNow - startTime;
            AddLog(result, "Error", "Exception", ex.Message);
            return result;
        }
    }

    public async Task<bool> DeprovisionTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deprovisioning tenant {TenantId}", tenantId);

            var namespace_ = $"{_options.NamespacePrefix}{tenantId}";
            var releaseName = $"tenant-{tenantId}";

            // Uninstall Helm release
            var (helmSuccess, helmOutput) = await _helmService.UninstallAsync(
                releaseName,
                namespace_,
                timeout: 300,
                cancellationToken: cancellationToken);

            if (!helmSuccess)
            {
                _logger.LogWarning("Failed to uninstall Helm release for tenant {TenantId}: {Output}",
                    tenantId, helmOutput);
            }

            // Delete namespace (this will clean up all resources)
            var namespaceDeleted = await _k8sClient.DeleteNamespaceAsync(namespace_, cancellationToken);

            if (!namespaceDeleted)
            {
                _logger.LogError("Failed to delete namespace for tenant {TenantId}", tenantId);
                return false;
            }

            _logger.LogInformation("Successfully deprovisioned tenant {TenantId}", tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deprovision tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<TenantProvisioningResult> UpdateTenantAsync(
        string tenantId,
        TenantProvisioningRequest request,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var result = new TenantProvisioningResult
        {
            TenantId = tenantId,
            Namespace = $"{_options.NamespacePrefix}{tenantId}",
            HelmRelease = $"tenant-{tenantId}",
            ProvisionedAt = startTime,
            SubscriptionTier = request.SubscriptionTier
        };

        try
        {
            AddLog(result, "Info", "UpdateTenant", $"Updating tenant {tenantId}");

            // Validate request
            var validation = await ValidateProvisioningRequestAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.ErrorMessage = $"Validation failed: {string.Join(", ", validation.Errors)}";
                return result;
            }

            // Check if namespace exists
            var namespaceExists = await _k8sClient.NamespaceExistsAsync(result.Namespace, cancellationToken);
            if (!namespaceExists)
            {
                result.Success = false;
                result.ErrorMessage = $"Tenant with ID '{tenantId}' does not exist";
                return result;
            }

            // Build updated Helm values
            var helmValues = BuildHelmValues(request, tenantId);

            // Upgrade Helm release
            var (helmSuccess, helmOutput) = await _helmService.UpgradeAsync(
                result.HelmRelease,
                _options.HelmChartPath,
                result.Namespace,
                helmValues,
                timeout: _options.ProvisioningTimeoutSeconds,
                cancellationToken: cancellationToken);

            if (!helmSuccess)
            {
                result.Success = false;
                result.ErrorMessage = $"Helm upgrade failed: {helmOutput}";
                AddLog(result, "Error", "UpgradeHelm", result.ErrorMessage);
                return result;
            }

            result.Success = true;
            result.ProvisioningDuration = DateTime.UtcNow - startTime;
            AddLog(result, "Info", "Complete", "Tenant updated successfully");

            _logger.LogInformation("Successfully updated tenant {TenantId}", tenantId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tenant {TenantId}", tenantId);
            result.Success = false;
            result.ErrorMessage = $"Unexpected error: {ex.Message}";
            result.ProvisioningDuration = DateTime.UtcNow - startTime;
            return result;
        }
    }

    public async Task<TenantStatus> GetTenantStatusAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var namespace_ = $"{_options.NamespacePrefix}{tenantId}";
        var status = new TenantStatus
        {
            TenantId = tenantId,
            Namespace = namespace_,
            LastChecked = DateTime.UtcNow
        };

        try
        {
            // Check if namespace exists
            var namespaceExists = await _k8sClient.NamespaceExistsAsync(namespace_, cancellationToken);
            if (!namespaceExists)
            {
                status.Status = "NotFound";
                return status;
            }

            // Get pod statuses
            status.Pods = await _k8sClient.GetPodStatusesAsync(namespace_, cancellationToken: cancellationToken);

            // Check database status
            var dbPods = status.Pods.Where(p => p.Name.Contains("postgres")).ToList();
            if (dbPods.Any())
            {
                status.Database = new DatabaseStatus
                {
                    IsReady = dbPods.All(p => p.Ready),
                    Status = dbPods.All(p => p.Ready) ? "Ready" : "NotReady",
                    Instances = dbPods.Count,
                    ReadyInstances = dbPods.Count(p => p.Ready)
                };
            }

            // Determine overall health
            status.IsHealthy = status.Pods.All(p => p.Ready);
            status.Status = status.IsHealthy ? "Healthy" : "Degraded";

            // Get tenant URL from ingress
            var ingresses = await _k8sClient.GetIngressesAsync(namespace_, cancellationToken);
            if (ingresses.Any())
            {
                var ingress = ingresses.First();
                var host = ingress.Spec.Rules?.FirstOrDefault()?.Host;
                if (!string.IsNullOrEmpty(host))
                {
                    status.TenantUrl = $"https://{host}";
                }
            }

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for tenant {TenantId}", tenantId);
            status.Status = "Error";
            return status;
        }
    }

    public async Task<ValidationResult> ValidateProvisioningRequestAsync(
        TenantProvisioningRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate tenant ID format if provided
        if (!string.IsNullOrEmpty(request.TenantId))
        {
            if (!IsValidTenantId(request.TenantId))
            {
                result.IsValid = false;
                result.Errors.Add("Invalid tenant ID format. Must be lowercase, alphanumeric with hyphens.");
            }

            // Check if tenant already exists
            var namespace_ = $"{_options.NamespacePrefix}{request.TenantId}";
            var exists = await _k8sClient.NamespaceExistsAsync(namespace_, cancellationToken);
            if (exists)
            {
                result.IsValid = false;
                result.Errors.Add($"Tenant with ID '{request.TenantId}' already exists.");
            }
        }

        // Validate subscription tier
        if (!_options.TierLimits.ContainsKey(request.SubscriptionTier))
        {
            result.IsValid = false;
            result.Errors.Add($"Invalid subscription tier: {request.SubscriptionTier}");
        }

        // Validate custom domain format
        if (!string.IsNullOrEmpty(request.CustomDomain))
        {
            if (!IsValidDomain(request.CustomDomain))
            {
                result.IsValid = false;
                result.Errors.Add("Invalid custom domain format.");
            }
        }

        // Check Kubernetes cluster accessibility
        var clusterAccessible = await _k8sClient.IsClusterAccessibleAsync(cancellationToken);
        if (!clusterAccessible)
        {
            result.IsValid = false;
            result.Errors.Add("Kubernetes cluster is not accessible.");
        }

        // Check Helm availability
        var helmAvailable = await _helmService.IsHelmAvailableAsync(cancellationToken);
        if (!helmAvailable)
        {
            result.IsValid = false;
            result.Errors.Add("Helm is not available.");
        }

        return result;
    }

    public string GenerateTenantId(string companyName)
    {
        // Convert to lowercase and remove special characters
        var slug = companyName.ToLowerInvariant();

        // Remove diacritics
        slug = RemoveDiacritics(slug);

        // Replace spaces and underscores with hyphens
        slug = Regex.Replace(slug, @"[\s_]+", "-");

        // Remove all non-alphanumeric characters except hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9-]", "");

        // Remove consecutive hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // Remove leading/trailing hyphens
        slug = slug.Trim('-');

        // Limit length to 20 characters
        if (slug.Length > 20)
        {
            slug = slug.Substring(0, 20).TrimEnd('-');
        }

        // Append short random suffix to ensure uniqueness
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        slug = $"{slug}-{suffix}";

        return slug;
    }

    private string BuildHelmValues(TenantProvisioningRequest request, string tenantId)
    {
        var tierLimits = GetTierLimits(request.SubscriptionTier, request.ResourceOverrides);

        var domain = string.IsNullOrEmpty(request.CustomDomain)
            ? $"{tenantId}.{_options.BaseDomain}"
            : request.CustomDomain;

        var values = new StringBuilder();
        values.AppendLine("# Auto-generated Helm values for tenant provisioning");
        values.AppendLine();

        // Tenant configuration
        values.AppendLine("tenant:");
        values.AppendLine($"  id: \"{tenantId}\"");
        values.AppendLine($"  name: \"{request.CompanyName}\"");
        values.AppendLine($"  tier: \"{request.SubscriptionTier}\"");
        values.AppendLine($"  ownerEmail: \"{request.OwnerEmail}\"");
        values.AppendLine();

        // Domain configuration
        values.AppendLine("domains:");
        values.AppendLine($"  baseDomain: \"{_options.BaseDomain}\"");
        values.AppendLine($"  default: \"{domain}\"");
        if (!string.IsNullOrEmpty(request.CustomDomain))
        {
            values.AppendLine("  custom:");
            values.AppendLine($"    - \"{request.CustomDomain}\"");
        }
        values.AppendLine($"  clusterIssuer: \"{_options.ClusterIssuer}\"");
        values.AppendLine();

        // Demo settings
        if (request.SubscriptionTier == "demo")
        {
            values.AppendLine("demo:");
            values.AppendLine("  enabled: true");
            if (tierLimits.ExpirationDays.HasValue)
            {
                values.AppendLine($"  expirationDays: {tierLimits.ExpirationDays.Value}");
            }
            values.AppendLine($"  populateSampleData: {request.PopulateSampleData.ToString().ToLower()}");
            values.AppendLine();
        }

        // PostgreSQL configuration
        values.AppendLine("postgresql:");
        values.AppendLine($"  instances: {tierLimits.PostgresInstances}");
        values.AppendLine("  database: \"mechanicbuddy\"");
        values.AppendLine("  username: \"mechanicbuddy\"");
        values.AppendLine("  storage:");
        values.AppendLine($"    size: \"{tierLimits.PostgresStorageSize}\"");
        values.AppendLine($"    storageClass: \"{tierLimits.StorageClass ?? _options.StorageClass}\"");
        values.AppendLine("  resources:");
        values.AppendLine("    requests:");
        values.AppendLine($"      memory: \"{tierLimits.PostgresMemoryRequest}\"");
        values.AppendLine($"      cpu: \"{tierLimits.PostgresCpuRequest}\"");
        values.AppendLine("    limits:");
        values.AppendLine($"      memory: \"{tierLimits.PostgresMemoryLimit}\"");
        values.AppendLine($"      cpu: \"{tierLimits.PostgresCpuLimit}\"");
        values.AppendLine("  backup:");
        values.AppendLine($"    enabled: {tierLimits.BackupEnabled.ToString().ToLower()}");
        values.AppendLine();

        // API configuration
        values.AppendLine("api:");
        values.AppendLine($"  replicas: {tierLimits.ApiReplicas}");
        values.AppendLine("  image:");
        values.AppendLine($"    repository: \"{_options.Registry.ApiRepository}\"");
        values.AppendLine($"    tag: \"{_options.Registry.DefaultTag}\"");
        values.AppendLine($"    pullPolicy: \"{_options.Registry.PullPolicy}\"");
        values.AppendLine("  resources:");
        values.AppendLine("    requests:");
        values.AppendLine($"      memory: \"{tierLimits.ApiMemoryRequest}\"");
        values.AppendLine($"      cpu: \"{tierLimits.ApiCpuRequest}\"");
        values.AppendLine("    limits:");
        values.AppendLine($"      memory: \"{tierLimits.ApiMemoryLimit}\"");
        values.AppendLine($"      cpu: \"{tierLimits.ApiCpuLimit}\"");

        // Additional environment variables for API
        if (request.AdditionalEnvVars?.Any() == true)
        {
            values.AppendLine("  extraEnv:");
            foreach (var env in request.AdditionalEnvVars)
            {
                values.AppendLine($"    - name: \"{env.Key}\"");
                values.AppendLine($"      value: \"{env.Value}\"");
            }
        }
        values.AppendLine();

        // Web configuration
        values.AppendLine("web:");
        values.AppendLine($"  replicas: {tierLimits.WebReplicas}");
        values.AppendLine("  image:");
        values.AppendLine($"    repository: \"{_options.Registry.WebRepository}\"");
        values.AppendLine($"    tag: \"{_options.Registry.DefaultTag}\"");
        values.AppendLine($"    pullPolicy: \"{_options.Registry.PullPolicy}\"");
        values.AppendLine("  resources:");
        values.AppendLine("    requests:");
        values.AppendLine($"      memory: \"{tierLimits.WebMemoryRequest}\"");
        values.AppendLine($"      cpu: \"{tierLimits.WebCpuRequest}\"");
        values.AppendLine("    limits:");
        values.AppendLine($"      memory: \"{tierLimits.WebMemoryLimit}\"");
        values.AppendLine($"      cpu: \"{tierLimits.WebCpuLimit}\"");
        values.AppendLine();

        // Migrations configuration
        values.AppendLine("migrations:");
        values.AppendLine("  enabled: true");
        values.AppendLine("  image:");
        values.AppendLine($"    repository: \"{_options.Registry.DbUpRepository}\"");
        values.AppendLine($"    tag: \"{_options.Registry.DefaultTag}\"");
        values.AppendLine($"    pullPolicy: \"{_options.Registry.PullPolicy}\"");
        values.AppendLine($"  timeout: {_options.MigrationTimeoutSeconds}");
        values.AppendLine();

        // Billing configuration
        if (!string.IsNullOrEmpty(request.StripeCustomerId))
        {
            values.AppendLine("billing:");
            values.AppendLine($"  stripeCustomerId: \"{request.StripeCustomerId}\"");
            if (!string.IsNullOrEmpty(request.StripeSubscriptionId))
            {
                values.AppendLine($"  subscriptionId: \"{request.StripeSubscriptionId}\"");
            }
            if (tierLimits.MechanicLimit.HasValue)
            {
                values.AppendLine($"  mechanicLimit: {tierLimits.MechanicLimit.Value}");
            }
            else
            {
                values.AppendLine("  mechanicLimit: null");
            }
            values.AppendLine();
        }

        return values.ToString();
    }

    private TierResourceLimits GetTierLimits(string tier, TenantResourceOverrides? overrides)
    {
        var limits = _options.TierLimits[tier];

        // Apply overrides if provided
        if (overrides != null)
        {
            if (overrides.PostgresInstances.HasValue)
                limits.PostgresInstances = overrides.PostgresInstances.Value;
            if (!string.IsNullOrEmpty(overrides.PostgresStorageSize))
                limits.PostgresStorageSize = overrides.PostgresStorageSize;
            if (overrides.ApiReplicas.HasValue)
                limits.ApiReplicas = overrides.ApiReplicas.Value;
            if (overrides.WebReplicas.HasValue)
                limits.WebReplicas = overrides.WebReplicas.Value;
            if (overrides.MechanicLimit.HasValue)
                limits.MechanicLimit = overrides.MechanicLimit.Value;
            if (!string.IsNullOrEmpty(overrides.StorageClass))
                limits.StorageClass = overrides.StorageClass;
        }

        return limits;
    }

    private async Task<bool> WaitForPostgresClusterAsync(
        string namespace_,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        // Wait for PostgreSQL cluster pods to be ready
        // CloudNativePG creates pods with the pattern: {cluster-name}-{instance}
        return await _k8sClient.WaitForPodsReadyAsync(
            namespace_,
            labelSelector: "cnpg.io/cluster",
            timeoutSeconds: timeoutSeconds,
            cancellationToken: cancellationToken);
    }

    private void AddLog(TenantProvisioningResult result, string level, string step, string message)
    {
        result.ProvisioningLog.Add(new ProvisioningLogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Step = step,
            Message = message
        });
    }

    private bool IsValidTenantId(string tenantId)
    {
        return Regex.IsMatch(tenantId, @"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$");
    }

    private bool IsValidDomain(string domain)
    {
        return Regex.IsMatch(domain, @"^[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$");
    }

    private string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public async Task<bool> SuspendTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Suspending tenant {TenantId}", tenantId);

            var namespace_ = $"{_options.NamespacePrefix}{tenantId}";

            // Check if namespace exists
            var namespaceExists = await _k8sClient.NamespaceExistsAsync(namespace_, cancellationToken);
            if (!namespaceExists)
            {
                _logger.LogWarning("Cannot suspend tenant {TenantId}: namespace not found", tenantId);
                return false;
            }

            // Scale all deployments to 0 replicas
            var scaled = await _k8sClient.ScaleAllDeploymentsAsync(
                namespace_,
                replicas: 0,
                cancellationToken: cancellationToken);

            if (!scaled)
            {
                _logger.LogError("Failed to scale deployments for tenant {TenantId}", tenantId);
                return false;
            }

            _logger.LogInformation("Successfully suspended tenant {TenantId}", tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to suspend tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> ResumeTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Resuming tenant {TenantId}", tenantId);

            var namespace_ = $"{_options.NamespacePrefix}{tenantId}";

            // Check if namespace exists
            var namespaceExists = await _k8sClient.NamespaceExistsAsync(namespace_, cancellationToken);
            if (!namespaceExists)
            {
                _logger.LogWarning("Cannot resume tenant {TenantId}: namespace not found", tenantId);
                return false;
            }

            // Get the Helm release to determine the original replica counts
            // We'll use label selectors to identify different components and scale them appropriately

            // Scale API deployment to default (1 replica minimum)
            var apiScaled = await _k8sClient.ScaleAllDeploymentsAsync(
                namespace_,
                replicas: 1,
                labelSelector: "app.kubernetes.io/component=api",
                cancellationToken: cancellationToken);

            // Scale Web deployment to default (1 replica minimum)
            var webScaled = await _k8sClient.ScaleAllDeploymentsAsync(
                namespace_,
                replicas: 1,
                labelSelector: "app.kubernetes.io/component=web",
                cancellationToken: cancellationToken);

            if (!apiScaled || !webScaled)
            {
                _logger.LogWarning("Some deployments failed to scale for tenant {TenantId}", tenantId);
                return false;
            }

            _logger.LogInformation("Successfully resumed tenant {TenantId}", tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume tenant {TenantId}", tenantId);
            return false;
        }
    }
}
