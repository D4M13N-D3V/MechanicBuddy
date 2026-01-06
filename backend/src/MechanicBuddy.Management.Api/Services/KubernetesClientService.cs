using k8s;
using k8s.Models;
using System.Text;

namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Service for Kubernetes client operations.
/// </summary>
public class KubernetesClientService : IKubernetesClientService
{
    private readonly IKubernetes _kubernetesClient;
    private readonly ILogger<KubernetesClientService> _logger;

    public KubernetesClientService(
        IKubernetes kubernetesClient,
        ILogger<KubernetesClientService> logger)
    {
        _kubernetesClient = kubernetesClient;
        _logger = logger;
    }

    public async Task<bool> CreateNamespaceAsync(
        string namespaceName,
        Dictionary<string, string>? labels = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating namespace {Namespace}", namespaceName);

            var namespace_ = new V1Namespace
            {
                Metadata = new V1ObjectMeta
                {
                    Name = namespaceName,
                    Labels = labels ?? new Dictionary<string, string>()
                }
            };

            // Add default labels
            namespace_.Metadata.Labels["app.kubernetes.io/managed-by"] = "mechanicbuddy-management";
            namespace_.Metadata.Labels["mechanicbuddy.app/tenant"] = namespaceName;

            await _kubernetesClient.CoreV1.CreateNamespaceAsync(namespace_, cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully created namespace {Namespace}", namespaceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create namespace {Namespace}", namespaceName);
            return false;
        }
    }

    public async Task<bool> DeleteNamespaceAsync(
        string namespaceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting namespace {Namespace}", namespaceName);

            await _kubernetesClient.CoreV1.DeleteNamespaceAsync(
                namespaceName,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully deleted namespace {Namespace}", namespaceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete namespace {Namespace}", namespaceName);
            return false;
        }
    }

    public async Task<bool> NamespaceExistsAsync(
        string namespaceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _kubernetesClient.CoreV1.ReadNamespaceAsync(namespaceName, cancellationToken: cancellationToken);
            return true;
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if namespace {Namespace} exists", namespaceName);
            return false;
        }
    }

    public async Task<V1Namespace?> GetNamespaceAsync(
        string namespaceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _kubernetesClient.CoreV1.ReadNamespaceAsync(
                namespaceName,
                cancellationToken: cancellationToken);
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting namespace {Namespace}", namespaceName);
            return null;
        }
    }

    public async Task<bool> WaitForPodsReadyAsync(
        string namespaceName,
        string? labelSelector = null,
        int timeoutSeconds = 300,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Waiting for pods in namespace {Namespace} to be ready (timeout: {Timeout}s)",
            namespaceName, timeoutSeconds);

        var startTime = DateTime.UtcNow;
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var pods = await _kubernetesClient.CoreV1.ListNamespacedPodAsync(
                    namespaceName,
                    labelSelector: labelSelector,
                    cancellationToken: cancellationToken);

                if (pods.Items.Count == 0)
                {
                    _logger.LogDebug("No pods found yet in namespace {Namespace}", namespaceName);
                    await Task.Delay(5000, cancellationToken);
                    continue;
                }

                var allReady = true;
                foreach (var pod in pods.Items)
                {
                    var podReady = IsPodReady(pod);
                    if (!podReady)
                    {
                        _logger.LogDebug("Pod {PodName} is not ready yet (phase: {Phase})",
                            pod.Metadata.Name, pod.Status.Phase);
                        allReady = false;
                    }
                }

                if (allReady)
                {
                    _logger.LogInformation("All pods in namespace {Namespace} are ready", namespaceName);
                    return true;
                }

                await Task.Delay(5000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while waiting for pods in namespace {Namespace}", namespaceName);
                await Task.Delay(5000, cancellationToken);
            }
        }

        _logger.LogWarning("Timeout waiting for pods in namespace {Namespace} to be ready", namespaceName);
        return false;
    }

    public async Task<bool> WaitForPodReadyAsync(
        string namespaceName,
        string podName,
        int timeoutSeconds = 300,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Waiting for pod {PodName} in namespace {Namespace} to be ready",
            podName, namespaceName);

        var startTime = DateTime.UtcNow;
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var pod = await _kubernetesClient.CoreV1.ReadNamespacedPodAsync(
                    podName,
                    namespaceName,
                    cancellationToken: cancellationToken);

                if (IsPodReady(pod))
                {
                    _logger.LogInformation("Pod {PodName} is ready", podName);
                    return true;
                }

                _logger.LogDebug("Pod {PodName} is not ready yet (phase: {Phase})",
                    podName, pod.Status.Phase);

                await Task.Delay(5000, cancellationToken);
            }
            catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Pod {PodName} not found yet", podName);
                await Task.Delay(5000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while waiting for pod {PodName}", podName);
                await Task.Delay(5000, cancellationToken);
            }
        }

        _logger.LogWarning("Timeout waiting for pod {PodName} to be ready", podName);
        return false;
    }

    public async Task<List<V1Pod>> ListPodsAsync(
        string namespaceName,
        string? labelSelector = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pods = await _kubernetesClient.CoreV1.ListNamespacedPodAsync(
                namespaceName,
                labelSelector: labelSelector,
                cancellationToken: cancellationToken);

            return pods.Items.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing pods in namespace {Namespace}", namespaceName);
            return new List<V1Pod>();
        }
    }

    public async Task<PodStatus?> GetPodStatusAsync(
        string namespaceName,
        string podName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pod = await _kubernetesClient.CoreV1.ReadNamespacedPodAsync(
                podName,
                namespaceName,
                cancellationToken: cancellationToken);

            return new PodStatus
            {
                Name = pod.Metadata.Name,
                Phase = pod.Status.Phase,
                Ready = IsPodReady(pod),
                ReadyContainers = pod.Status.ContainerStatuses?.Count(c => c.Ready) ?? 0,
                TotalContainers = pod.Status.ContainerStatuses?.Count ?? 0,
                ContainerStatuses = pod.Status.ContainerStatuses?.Select(c =>
                    $"{c.Name}: {(c.Ready ? "Ready" : "Not Ready")}").ToList() ?? new List<string>(),
                StartTime = pod.Status.StartTime,
                Message = pod.Status.Message
            };
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pod status for {PodName} in namespace {Namespace}",
                podName, namespaceName);
            return null;
        }
    }

    public async Task<List<PodStatus>> GetPodStatusesAsync(
        string namespaceName,
        string? labelSelector = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pods = await _kubernetesClient.CoreV1.ListNamespacedPodAsync(
                namespaceName,
                labelSelector: labelSelector,
                cancellationToken: cancellationToken);

            return pods.Items.Select(pod => new PodStatus
            {
                Name = pod.Metadata.Name,
                Phase = pod.Status.Phase,
                Ready = IsPodReady(pod),
                ReadyContainers = pod.Status.ContainerStatuses?.Count(c => c.Ready) ?? 0,
                TotalContainers = pod.Status.ContainerStatuses?.Count ?? 0,
                ContainerStatuses = pod.Status.ContainerStatuses?.Select(c =>
                    $"{c.Name}: {(c.Ready ? "Ready" : "Not Ready")}").ToList() ?? new List<string>(),
                StartTime = pod.Status.StartTime,
                Message = pod.Status.Message
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pod statuses in namespace {Namespace}", namespaceName);
            return new List<PodStatus>();
        }
    }

    public async Task<bool> CreateSecretAsync(
        string namespaceName,
        string secretName,
        Dictionary<string, string> data,
        string type = "Opaque",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating secret {SecretName} in namespace {Namespace}",
                secretName, namespaceName);

            // Convert string data to base64
            var encodedData = data.ToDictionary(
                kvp => kvp.Key,
                kvp => Encoding.UTF8.GetBytes(kvp.Value));

            var secret = new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = secretName,
                    NamespaceProperty = namespaceName
                },
                Type = type,
                Data = encodedData
            };

            await _kubernetesClient.CoreV1.CreateNamespacedSecretAsync(
                secret,
                namespaceName,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully created secret {SecretName}", secretName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create secret {SecretName}", secretName);
            return false;
        }
    }

    public async Task<bool> DeleteSecretAsync(
        string namespaceName,
        string secretName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting secret {SecretName} from namespace {Namespace}",
                secretName, namespaceName);

            await _kubernetesClient.CoreV1.DeleteNamespacedSecretAsync(
                secretName,
                namespaceName,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully deleted secret {SecretName}", secretName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete secret {SecretName}", secretName);
            return false;
        }
    }

    public async Task<V1Secret?> GetSecretAsync(
        string namespaceName,
        string secretName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _kubernetesClient.CoreV1.ReadNamespacedSecretAsync(
                secretName,
                namespaceName,
                cancellationToken: cancellationToken);
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting secret {SecretName}", secretName);
            return null;
        }
    }

    public async Task<List<V1Ingress>> GetIngressesAsync(
        string namespaceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ingresses = await _kubernetesClient.NetworkingV1.ListNamespacedIngressAsync(
                namespaceName,
                cancellationToken: cancellationToken);

            return ingresses.Items.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ingresses in namespace {Namespace}", namespaceName);
            return new List<V1Ingress>();
        }
    }

    public async Task<bool> IsClusterAccessibleAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var version = await _kubernetesClient.Version.GetCodeAsync(cancellationToken);
            _logger.LogInformation("Kubernetes cluster is accessible (version: {Version})",
                version.GitVersion);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kubernetes cluster is not accessible");
            return false;
        }
    }

    public async Task<bool> ScaleDeploymentAsync(
        string namespaceName,
        string deploymentName,
        int replicas,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Scaling deployment {DeploymentName} in namespace {Namespace} to {Replicas} replicas",
                deploymentName, namespaceName, replicas);

            var deployment = await _kubernetesClient.AppsV1.ReadNamespacedDeploymentAsync(
                deploymentName,
                namespaceName,
                cancellationToken: cancellationToken);

            deployment.Spec.Replicas = replicas;

            await _kubernetesClient.AppsV1.ReplaceNamespacedDeploymentAsync(
                deployment,
                deploymentName,
                namespaceName,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully scaled deployment {DeploymentName} to {Replicas} replicas",
                deploymentName, replicas);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scale deployment {DeploymentName} in namespace {Namespace}",
                deploymentName, namespaceName);
            return false;
        }
    }

    public async Task<bool> ScaleAllDeploymentsAsync(
        string namespaceName,
        int replicas,
        string? labelSelector = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Scaling all deployments in namespace {Namespace} to {Replicas} replicas",
                namespaceName, replicas);

            var deployments = await _kubernetesClient.AppsV1.ListNamespacedDeploymentAsync(
                namespaceName,
                labelSelector: labelSelector,
                cancellationToken: cancellationToken);

            var allSuccess = true;
            foreach (var deployment in deployments.Items)
            {
                var success = await ScaleDeploymentAsync(
                    namespaceName,
                    deployment.Metadata.Name,
                    replicas,
                    cancellationToken);

                if (!success)
                {
                    allSuccess = false;
                }
            }

            if (allSuccess)
            {
                _logger.LogInformation("Successfully scaled all deployments in namespace {Namespace}", namespaceName);
            }
            else
            {
                _logger.LogWarning("Some deployments failed to scale in namespace {Namespace}", namespaceName);
            }

            return allSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scale deployments in namespace {Namespace}", namespaceName);
            return false;
        }
    }

    public async Task<V1Ingress?> GetIngressAsync(
        string namespaceName,
        string ingressName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _kubernetesClient.NetworkingV1.ReadNamespacedIngressAsync(
                ingressName,
                namespaceName,
                cancellationToken: cancellationToken);
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ingress {IngressName} in namespace {Namespace}",
                ingressName, namespaceName);
            return null;
        }
    }

    public async Task<bool> UpdateIngressDomainsAsync(
        string namespaceName,
        string ingressName,
        List<string> domains,
        string clusterIssuer = "letsencrypt-prod",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating Ingress {IngressName} in namespace {Namespace} with domains: {Domains}",
                ingressName, namespaceName, string.Join(", ", domains));

            // Get the existing Ingress
            var ingress = await GetIngressAsync(namespaceName, ingressName, cancellationToken);
            if (ingress == null)
            {
                _logger.LogError("Ingress {IngressName} not found in namespace {Namespace}",
                    ingressName, namespaceName);
                return false;
            }

            // Update annotations for cert-manager
            if (ingress.Metadata.Annotations == null)
            {
                ingress.Metadata.Annotations = new Dictionary<string, string>();
            }

            ingress.Metadata.Annotations["cert-manager.io/cluster-issuer"] = clusterIssuer;
            ingress.Metadata.Annotations["cert-manager.io/acme-challenge-type"] = "http01";

            // Update rules to include all domains
            if (ingress.Spec.Rules == null)
            {
                ingress.Spec.Rules = new List<V1IngressRule>();
            }

            // Get the first rule as a template (assumes existing rule has correct backend config)
            var templateRule = ingress.Spec.Rules.FirstOrDefault();
            if (templateRule == null)
            {
                _logger.LogError("No existing rules found in Ingress {IngressName}", ingressName);
                return false;
            }

            // Clear existing rules and rebuild with all domains
            ingress.Spec.Rules.Clear();

            foreach (var domain in domains)
            {
                var rule = new V1IngressRule
                {
                    Host = domain,
                    Http = templateRule.Http // Reuse the same HTTP config
                };
                ingress.Spec.Rules.Add(rule);
            }

            // Update TLS configuration
            if (ingress.Spec.Tls == null)
            {
                ingress.Spec.Tls = new List<V1IngressTLS>();
            }

            // Create TLS entries for each domain
            ingress.Spec.Tls.Clear();
            foreach (var domain in domains)
            {
                var tls = new V1IngressTLS
                {
                    Hosts = new List<string> { domain },
                    SecretName = $"{domain.Replace(".", "-")}-tls"
                };
                ingress.Spec.Tls.Add(tls);
            }

            // Replace the Ingress
            await _kubernetesClient.NetworkingV1.ReplaceNamespacedIngressAsync(
                ingress,
                ingressName,
                namespaceName,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully updated Ingress {IngressName} with custom domains", ingressName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Ingress {IngressName} in namespace {Namespace}",
                ingressName, namespaceName);
            return false;
        }
    }

    private bool IsPodReady(V1Pod pod)
    {
        // Check if pod is in Running phase
        if (pod.Status.Phase != "Running")
        {
            return false;
        }

        // Check if all containers are ready
        if (pod.Status.ContainerStatuses == null || pod.Status.ContainerStatuses.Count == 0)
        {
            return false;
        }

        return pod.Status.ContainerStatuses.All(c => c.Ready);
    }
}
