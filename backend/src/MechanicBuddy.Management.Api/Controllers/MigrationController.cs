using MechanicBuddy.Management.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicBuddy.Management.Api.Controllers;

/// <summary>
/// Controller for tenant migration operations.
/// Handles migrating tenants between shared and dedicated deployments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly")]
public class MigrationController : ControllerBase
{
    private readonly ITenantMigrationService _migrationService;
    private readonly ILogger<MigrationController> _logger;

    public MigrationController(
        ITenantMigrationService migrationService,
        ILogger<MigrationController> logger)
    {
        _migrationService = migrationService;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a tenant is eligible for migration.
    /// </summary>
    /// <param name="tenantId">The tenant ID to check.</param>
    /// <returns>Migration eligibility information.</returns>
    [HttpGet("{tenantId}/eligibility")]
    [ProducesResponseType(typeof(MigrationEligibility), StatusCodes.Status200OK)]
    public async Task<ActionResult<MigrationEligibility>> CheckEligibility(string tenantId)
    {
        var result = await _migrationService.CheckMigrationEligibilityAsync(tenantId);
        return Ok(result);
    }

    /// <summary>
    /// Migrates a tenant from dedicated infrastructure to shared free-tier instance.
    /// </summary>
    /// <param name="tenantId">The tenant ID to migrate.</param>
    /// <returns>Migration result.</returns>
    [HttpPost("{tenantId}/to-shared")]
    [ProducesResponseType(typeof(MigrationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MigrationResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MigrationResult>> MigrateToShared(string tenantId)
    {
        _logger.LogInformation("Received request to migrate tenant {TenantId} to shared instance", tenantId);

        var result = await _migrationService.MigrateToSharedInstanceAsync(tenantId);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Migrates a tenant from shared instance to dedicated infrastructure (upgrade).
    /// </summary>
    /// <param name="tenantId">The tenant ID to migrate.</param>
    /// <param name="request">Upgrade request with target tier.</param>
    /// <returns>Migration result.</returns>
    [HttpPost("{tenantId}/to-dedicated")]
    [ProducesResponseType(typeof(MigrationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MigrationResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MigrationResult>> MigrateToDedicated(
        string tenantId,
        [FromBody] UpgradeRequest request)
    {
        _logger.LogInformation("Received request to migrate tenant {TenantId} to dedicated instance ({Tier})",
            tenantId, request.TargetTier);

        var result = await _migrationService.MigrateToDedicatedInstanceAsync(tenantId, request.TargetTier);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Bulk migrates multiple tenants to shared instance.
    /// </summary>
    /// <param name="request">Bulk migration request with tenant IDs.</param>
    /// <returns>Bulk migration result.</returns>
    [HttpPost("bulk/to-shared")]
    [ProducesResponseType(typeof(BulkMigrationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<BulkMigrationResult>> BulkMigrateToShared(
        [FromBody] BulkMigrationRequest request)
    {
        _logger.LogInformation("Received request to bulk migrate {Count} tenants to shared instance",
            request.TenantIds?.Count ?? 0);

        if (request.TenantIds == null || !request.TenantIds.Any())
        {
            return BadRequest(new BulkMigrationResult
            {
                TotalRequested = 0,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            });
        }

        var result = await _migrationService.BulkMigrateToSharedInstanceAsync(request.TenantIds);
        return Ok(result);
    }
}

/// <summary>
/// Request model for upgrading a tenant to dedicated instance.
/// </summary>
public class UpgradeRequest
{
    /// <summary>
    /// Target subscription tier (e.g., "professional", "enterprise").
    /// </summary>
    public string TargetTier { get; set; } = "professional";
}

/// <summary>
/// Request model for bulk migration.
/// </summary>
public class BulkMigrationRequest
{
    /// <summary>
    /// List of tenant IDs to migrate.
    /// </summary>
    public List<string>? TenantIds { get; set; }
}
