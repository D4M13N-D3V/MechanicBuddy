namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Interface for Helm chart management operations.
/// </summary>
public interface IHelmService
{
    /// <summary>
    /// Installs a Helm chart.
    /// </summary>
    /// <param name="releaseName">Name of the Helm release.</param>
    /// <param name="chartPath">Path to the Helm chart directory.</param>
    /// <param name="namespace">Kubernetes namespace.</param>
    /// <param name="values">Helm values as YAML string.</param>
    /// <param name="createNamespace">Whether to create the namespace if it doesn't exist.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of success status and output/error message.</returns>
    Task<(bool Success, string Output)> InstallAsync(
        string releaseName,
        string chartPath,
        string namespace_,
        string values,
        bool createNamespace = true,
        int timeout = 300,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upgrades an existing Helm release.
    /// </summary>
    /// <param name="releaseName">Name of the Helm release.</param>
    /// <param name="chartPath">Path to the Helm chart directory.</param>
    /// <param name="namespace">Kubernetes namespace.</param>
    /// <param name="values">Helm values as YAML string.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of success status and output/error message.</returns>
    Task<(bool Success, string Output)> UpgradeAsync(
        string releaseName,
        string chartPath,
        string namespace_,
        string values,
        int timeout = 300,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uninstalls a Helm release.
    /// </summary>
    /// <param name="releaseName">Name of the Helm release.</param>
    /// <param name="namespace">Kubernetes namespace.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of success status and output/error message.</returns>
    Task<(bool Success, string Output)> UninstallAsync(
        string releaseName,
        string namespace_,
        int timeout = 300,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a Helm release.
    /// </summary>
    /// <param name="releaseName">Name of the Helm release.</param>
    /// <param name="namespace">Kubernetes namespace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of success status and output/error message.</returns>
    Task<(bool Success, string Output)> GetStatusAsync(
        string releaseName,
        string namespace_,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists Helm releases in a namespace.
    /// </summary>
    /// <param name="namespace">Kubernetes namespace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of success status and output/error message.</returns>
    Task<(bool Success, string Output)> ListReleasesAsync(
        string namespace_,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if Helm is installed and accessible.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if Helm is available.</returns>
    Task<bool> IsHelmAvailableAsync(CancellationToken cancellationToken = default);
}
