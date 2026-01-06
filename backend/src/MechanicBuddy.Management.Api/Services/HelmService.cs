using System.Diagnostics;
using System.Text;

namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Service for executing Helm CLI commands.
/// </summary>
public class HelmService : IHelmService
{
    private readonly ILogger<HelmService> _logger;
    private const string HelmCommand = "helm";

    public HelmService(ILogger<HelmService> logger)
    {
        _logger = logger;
    }

    public async Task<(bool Success, string Output)> InstallAsync(
        string releaseName,
        string chartPath,
        string namespace_,
        string values,
        bool createNamespace = true,
        int timeout = 300,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Installing Helm release {ReleaseName} in namespace {Namespace}",
            releaseName, namespace_);

        try
        {
            // Write values to a temporary file
            var valuesFile = Path.Combine(Path.GetTempPath(), $"helm-values-{Guid.NewGuid()}.yaml");
            await File.WriteAllTextAsync(valuesFile, values, cancellationToken);

            try
            {
                var args = new List<string>
                {
                    "install",
                    releaseName,
                    chartPath,
                    "--namespace", namespace_,
                    "--values", valuesFile,
                    "--timeout", $"{timeout}s",
                    "--wait",
                    "--wait-for-jobs"
                };

                if (createNamespace)
                {
                    args.Add("--create-namespace");
                }

                var (exitCode, output, error) = await ExecuteHelmCommandAsync(args, cancellationToken);

                if (exitCode == 0)
                {
                    _logger.LogInformation("Successfully installed Helm release {ReleaseName}", releaseName);
                    return (true, output);
                }
                else
                {
                    _logger.LogError("Failed to install Helm release {ReleaseName}: {Error}",
                        releaseName, error);
                    return (false, error);
                }
            }
            finally
            {
                // Clean up temporary values file
                if (File.Exists(valuesFile))
                {
                    File.Delete(valuesFile);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while installing Helm release {ReleaseName}", releaseName);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Output)> UpgradeAsync(
        string releaseName,
        string chartPath,
        string namespace_,
        string values,
        int timeout = 300,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Upgrading Helm release {ReleaseName} in namespace {Namespace}",
            releaseName, namespace_);

        try
        {
            // Write values to a temporary file
            var valuesFile = Path.Combine(Path.GetTempPath(), $"helm-values-{Guid.NewGuid()}.yaml");
            await File.WriteAllTextAsync(valuesFile, values, cancellationToken);

            try
            {
                var args = new List<string>
                {
                    "upgrade",
                    releaseName,
                    chartPath,
                    "--namespace", namespace_,
                    "--values", valuesFile,
                    "--timeout", $"{timeout}s",
                    "--wait",
                    "--wait-for-jobs",
                    "--install" // Create if not exists
                };

                var (exitCode, output, error) = await ExecuteHelmCommandAsync(args, cancellationToken);

                if (exitCode == 0)
                {
                    _logger.LogInformation("Successfully upgraded Helm release {ReleaseName}", releaseName);
                    return (true, output);
                }
                else
                {
                    _logger.LogError("Failed to upgrade Helm release {ReleaseName}: {Error}",
                        releaseName, error);
                    return (false, error);
                }
            }
            finally
            {
                // Clean up temporary values file
                if (File.Exists(valuesFile))
                {
                    File.Delete(valuesFile);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while upgrading Helm release {ReleaseName}", releaseName);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Output)> UninstallAsync(
        string releaseName,
        string namespace_,
        int timeout = 300,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uninstalling Helm release {ReleaseName} from namespace {Namespace}",
            releaseName, namespace_);

        try
        {
            var args = new List<string>
            {
                "uninstall",
                releaseName,
                "--namespace", namespace_,
                "--timeout", $"{timeout}s",
                "--wait"
            };

            var (exitCode, output, error) = await ExecuteHelmCommandAsync(args, cancellationToken);

            if (exitCode == 0)
            {
                _logger.LogInformation("Successfully uninstalled Helm release {ReleaseName}", releaseName);
                return (true, output);
            }
            else
            {
                _logger.LogError("Failed to uninstall Helm release {ReleaseName}: {Error}",
                    releaseName, error);
                return (false, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while uninstalling Helm release {ReleaseName}", releaseName);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Output)> GetStatusAsync(
        string releaseName,
        string namespace_,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new List<string>
            {
                "status",
                releaseName,
                "--namespace", namespace_
            };

            var (exitCode, output, error) = await ExecuteHelmCommandAsync(args, cancellationToken);

            if (exitCode == 0)
            {
                return (true, output);
            }
            else
            {
                return (false, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while getting status of Helm release {ReleaseName}", releaseName);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Output)> ListReleasesAsync(
        string namespace_,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new List<string>
            {
                "list",
                "--namespace", namespace_,
                "--output", "json"
            };

            var (exitCode, output, error) = await ExecuteHelmCommandAsync(args, cancellationToken);

            if (exitCode == 0)
            {
                return (true, output);
            }
            else
            {
                return (false, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while listing Helm releases in namespace {Namespace}", namespace_);
            return (false, ex.Message);
        }
    }

    public async Task<bool> IsHelmAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new List<string> { "version", "--short" };
            var (exitCode, output, _) = await ExecuteHelmCommandAsync(args, cancellationToken);

            if (exitCode == 0)
            {
                _logger.LogInformation("Helm is available: {Version}", output.Trim());
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check Helm availability");
            return false;
        }
    }

    private async Task<(int ExitCode, string Output, string Error)> ExecuteHelmCommandAsync(
        List<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = HelmCommand,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                outputBuilder.AppendLine(e.Data);
                _logger.LogDebug("Helm stdout: {Output}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
                _logger.LogDebug("Helm stderr: {Error}", e.Data);
            }
        };

        _logger.LogDebug("Executing Helm command: {Command} {Args}",
            HelmCommand, string.Join(" ", arguments));

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        _logger.LogDebug("Helm command exited with code {ExitCode}", process.ExitCode);

        return (process.ExitCode, output, error);
    }
}
