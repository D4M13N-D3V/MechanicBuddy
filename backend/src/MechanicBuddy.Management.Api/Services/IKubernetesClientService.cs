using k8s.Models;

namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Interface for Kubernetes client operations.
/// </summary>
public interface IKubernetesClientService
{
    /// <summary>
    /// Creates a Kubernetes namespace.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace to create.</param>
    /// <param name="labels">Optional labels to apply to the namespace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if namespace was created successfully.</returns>
    Task<bool> CreateNamespaceAsync(
        string namespaceName,
        Dictionary<string, string>? labels = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a Kubernetes namespace.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if namespace was deleted successfully.</returns>
    Task<bool> DeleteNamespaceAsync(
        string namespaceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a namespace exists.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if namespace exists.</returns>
    Task<bool> NamespaceExistsAsync(
        string namespaceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a namespace.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Namespace object or null if not found.</returns>
    Task<V1Namespace?> GetNamespaceAsync(
        string namespaceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for all pods in a namespace to be ready.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="labelSelector">Optional label selector to filter pods.</param>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if all pods are ready within timeout.</returns>
    Task<bool> WaitForPodsReadyAsync(
        string namespaceName,
        string? labelSelector = null,
        int timeoutSeconds = 300,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a specific pod to be ready.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="podName">Name of the pod.</param>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if pod is ready within timeout.</returns>
    Task<bool> WaitForPodReadyAsync(
        string namespaceName,
        string podName,
        int timeoutSeconds = 300,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all pods in a namespace.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="labelSelector">Optional label selector to filter pods.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pods.</returns>
    Task<List<V1Pod>> ListPodsAsync(
        string namespaceName,
        string? labelSelector = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a specific pod.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="podName">Name of the pod.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pod status or null if not found.</returns>
    Task<PodStatus?> GetPodStatusAsync(
        string namespaceName,
        string podName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of pods in a namespace.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="labelSelector">Optional label selector to filter pods.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pod statuses.</returns>
    Task<List<PodStatus>> GetPodStatusesAsync(
        string namespaceName,
        string? labelSelector = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Kubernetes secret.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="secretName">Name of the secret.</param>
    /// <param name="data">Secret data (will be base64 encoded).</param>
    /// <param name="type">Secret type (default: Opaque).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if secret was created successfully.</returns>
    Task<bool> CreateSecretAsync(
        string namespaceName,
        string secretName,
        Dictionary<string, string> data,
        string type = "Opaque",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a Kubernetes secret.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="secretName">Name of the secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if secret was deleted successfully.</returns>
    Task<bool> DeleteSecretAsync(
        string namespaceName,
        string secretName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a Kubernetes secret.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="secretName">Name of the secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Secret object or null if not found.</returns>
    Task<V1Secret?> GetSecretAsync(
        string namespaceName,
        string secretName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets ingresses in a namespace.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of ingresses.</returns>
    Task<List<V1Ingress>> GetIngressesAsync(
        string namespaceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the Kubernetes cluster is accessible.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if cluster is accessible.</returns>
    Task<bool> IsClusterAccessibleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Scales a deployment to the specified number of replicas.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="deploymentName">Name of the deployment.</param>
    /// <param name="replicas">Target number of replicas.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if scaling was successful.</returns>
    Task<bool> ScaleDeploymentAsync(
        string namespaceName,
        string deploymentName,
        int replicas,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scales all deployments in a namespace to the specified number of replicas.
    /// </summary>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="replicas">Target number of replicas.</param>
    /// <param name="labelSelector">Optional label selector to filter deployments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if all deployments were scaled successfully.</returns>
    Task<bool> ScaleAllDeploymentsAsync(
        string namespaceName,
        int replicas,
        string? labelSelector = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Pod status information.
/// </summary>
public class PodStatus
{
    public string Name { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public bool Ready { get; set; }
    public int ReadyContainers { get; set; }
    public int TotalContainers { get; set; }
    public List<string> ContainerStatuses { get; set; } = new();
    public DateTime? StartTime { get; set; }
    public string? Message { get; set; }
}
