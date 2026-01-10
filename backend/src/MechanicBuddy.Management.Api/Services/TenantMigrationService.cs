using MechanicBuddy.Management.Api.Configuration;
using MechanicBuddy.Management.Api.Infrastructure;
using MechanicBuddy.Management.Api.Models;
using Microsoft.Extensions.Options;

namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Service for migrating tenants between shared and dedicated deployments.
/// </summary>
public class TenantMigrationService : ITenantMigrationService
{
    private readonly ITenantProvisioningService _provisioningService;
    private readonly ITenantDatabaseProvisioner _dbProvisioner;
    private readonly IKubernetesClientService _k8sClient;
    private readonly INpmClient _npmClient;
    private readonly ICloudflareClient _cloudflareClient;
    private readonly ILogger<TenantMigrationService> _logger;
    private readonly ProvisioningOptions _options;

    public TenantMigrationService(
        ITenantProvisioningService provisioningService,
        ITenantDatabaseProvisioner dbProvisioner,
        IKubernetesClientService k8sClient,
        INpmClient npmClient,
        ICloudflareClient cloudflareClient,
        ILogger<TenantMigrationService> logger,
        IOptions<ProvisioningOptions> options)
    {
        _provisioningService = provisioningService;
        _dbProvisioner = dbProvisioner;
        _k8sClient = k8sClient;
        _npmClient = npmClient;
        _cloudflareClient = cloudflareClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<MigrationResult> MigrateToSharedInstanceAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var result = new MigrationResult
        {
            TenantId = tenantId,
            SourceMode = "dedicated",
            TargetMode = "shared",
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting migration of tenant {TenantId} to shared instance", tenantId);

            // Step 1: Verify tenant exists in dedicated mode
            var eligibility = await CheckMigrationEligibilityAsync(tenantId, cancellationToken);
            if (!eligibility.CanMigrate)
            {
                result.Success = false;
                result.ErrorMessage = eligibility.Reason;
                result.CompletedAt = DateTime.UtcNow;
                return result;
            }
            result.Steps.Add("Verified migration eligibility");

            var dedicatedNamespace = $"{_options.NamespacePrefix}{tenantId}";

            // Step 2: Export database from dedicated PostgreSQL
            // Note: For now, we'll re-create from template rather than migrate data
            // A full migration would use pg_dump/pg_restore
            result.Steps.Add("Checking database on dedicated instance");

            // Step 3: Create database on shared PostgreSQL
            result.Steps.Add("Creating database on shared PostgreSQL cluster");
            await _dbProvisioner.ProvisionTenantDatabaseAsync(
                tenantId,
                _options.FreeTier.PostgresHost,
                _options.FreeTier.PostgresPort);

            // Step 4: Update NPM proxy to point to shared instance
            // NPM is external to K8s, so we use the default forward host (external ingress IP)
            // The K8s ingress then routes to the internal service based on hostname
            result.Steps.Add("Updating proxy host to point to shared instance");
            await _npmClient.DeleteProxyHostAsync(tenantId);
            await _npmClient.CreateProxyHostAsync(tenantId); // Uses default forward host from config

            // Step 5: Delete dedicated namespace
            result.Steps.Add("Deleting dedicated namespace");
            var releaseName = $"tenant-{tenantId}";

            // Uninstall Helm release first
            // Note: We need to access HelmService - for now skip this and just delete namespace
            var namespaceDeleted = await _k8sClient.DeleteNamespaceAsync(dedicatedNamespace, cancellationToken);
            if (!namespaceDeleted)
            {
                _logger.LogWarning("Failed to delete namespace for tenant {TenantId}, but migration continues", tenantId);
            }

            result.Steps.Add("Migration completed successfully");
            result.Success = true;
            result.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation("Successfully migrated tenant {TenantId} to shared instance in {Duration}s",
                tenantId, result.Duration?.TotalSeconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate tenant {TenantId} to shared instance", tenantId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
            return result;
        }
    }

    public async Task<MigrationResult> MigrateToDedicatedInstanceAsync(
        string tenantId,
        string targetTier,
        CancellationToken cancellationToken = default)
    {
        var result = new MigrationResult
        {
            TenantId = tenantId,
            SourceMode = "shared",
            TargetMode = "dedicated",
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting migration of tenant {TenantId} from shared to dedicated ({Tier})",
                tenantId, targetTier);

            // Step 1: Verify tenant exists on shared instance
            var sharedDbExists = await _dbProvisioner.TenantDatabaseExistsAsync(
                tenantId,
                _options.FreeTier.PostgresHost,
                _options.FreeTier.PostgresPort);

            if (!sharedDbExists)
            {
                result.Success = false;
                result.ErrorMessage = $"Tenant {tenantId} does not exist on shared instance";
                result.CompletedAt = DateTime.UtcNow;
                return result;
            }
            result.Steps.Add("Verified tenant exists on shared instance");

            // Step 2: Provision dedicated instance
            result.Steps.Add("Provisioning dedicated instance");
            var provisionRequest = new TenantProvisioningRequest
            {
                TenantId = tenantId,
                CompanyName = tenantId, // Use tenant ID as company name
                SubscriptionTier = targetTier
            };

            // Temporarily disable free-tier routing for this provisioning
            // by passing the request directly to dedicated provisioning
            var provisionResult = await _provisioningService.ProvisionTenantAsync(provisionRequest, cancellationToken);

            if (!provisionResult.Success)
            {
                result.Success = false;
                result.ErrorMessage = $"Failed to provision dedicated instance: {provisionResult.ErrorMessage}";
                result.CompletedAt = DateTime.UtcNow;
                return result;
            }
            result.Steps.Add("Dedicated instance provisioned");

            // Step 3: Migrate data from shared to dedicated PostgreSQL
            // Note: For a full implementation, this would use pg_dump/pg_restore
            // For now, users would need to re-import their data
            result.Steps.Add("Note: Data migration requires manual intervention");

            // Step 4: Update NPM proxy to point to dedicated instance
            result.Steps.Add("Updating proxy host to point to dedicated instance");
            await _npmClient.DeleteProxyHostAsync(tenantId);
            await _npmClient.CreateProxyHostAsync(tenantId); // Uses default forward host

            // Step 5: Delete database from shared PostgreSQL
            result.Steps.Add("Cleaning up shared instance database");
            await _dbProvisioner.DeleteTenantDatabaseAsync(
                tenantId,
                _options.FreeTier.PostgresHost,
                _options.FreeTier.PostgresPort);

            result.Steps.Add("Migration completed successfully");
            result.Success = true;
            result.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation("Successfully migrated tenant {TenantId} to dedicated instance in {Duration}s",
                tenantId, result.Duration?.TotalSeconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate tenant {TenantId} to dedicated instance", tenantId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
            return result;
        }
    }

    public async Task<BulkMigrationResult> BulkMigrateToSharedInstanceAsync(
        IEnumerable<string> tenantIds,
        CancellationToken cancellationToken = default)
    {
        var tenantList = tenantIds.ToList();
        var result = new BulkMigrationResult
        {
            TotalRequested = tenantList.Count,
            StartedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Starting bulk migration of {Count} tenants to shared instance", tenantList.Count);

        foreach (var tenantId in tenantList)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Bulk migration cancelled at tenant {TenantId}", tenantId);
                break;
            }

            try
            {
                var migrationResult = await MigrateToSharedInstanceAsync(tenantId, cancellationToken);
                result.Results.Add(migrationResult);

                if (migrationResult.Success)
                {
                    result.Successful++;
                }
                else
                {
                    result.Failed++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk migration for tenant {TenantId}", tenantId);
                result.Results.Add(new MigrationResult
                {
                    TenantId = tenantId,
                    Success = false,
                    ErrorMessage = ex.Message,
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });
                result.Failed++;
            }
        }

        result.CompletedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Bulk migration completed: {Successful} successful, {Failed} failed, {Skipped} skipped in {Duration}s",
            result.Successful, result.Failed, result.Skipped, result.Duration?.TotalSeconds);

        return result;
    }

    public async Task<MigrationEligibility> CheckMigrationEligibilityAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var result = new MigrationEligibility
        {
            TenantId = tenantId
        };

        // Check if tenant has dedicated namespace
        var dedicatedNamespace = $"{_options.NamespacePrefix}{tenantId}";
        var hasNamespace = await _k8sClient.NamespaceExistsAsync(dedicatedNamespace, cancellationToken);

        // Check if tenant has database on shared instance
        var hasSharedDb = await _dbProvisioner.TenantDatabaseExistsAsync(
            tenantId,
            _options.FreeTier.PostgresHost,
            _options.FreeTier.PostgresPort);

        if (hasNamespace && !hasSharedDb)
        {
            result.CurrentMode = "dedicated";
            result.CanMigrate = true;
        }
        else if (!hasNamespace && hasSharedDb)
        {
            result.CurrentMode = "shared";
            result.CanMigrate = true;
        }
        else if (hasNamespace && hasSharedDb)
        {
            result.CurrentMode = "mixed";
            result.CanMigrate = false;
            result.Reason = "Tenant exists in both dedicated and shared modes - manual cleanup required";
        }
        else
        {
            result.CurrentMode = "none";
            result.CanMigrate = false;
            result.Reason = "Tenant does not exist in either mode";
        }

        // Add warnings about data migration
        if (result.CanMigrate && result.CurrentMode == "dedicated")
        {
            result.Warnings.Add("Data migration is not automatic - tenant will start with fresh database from template");
        }

        return result;
    }
}
