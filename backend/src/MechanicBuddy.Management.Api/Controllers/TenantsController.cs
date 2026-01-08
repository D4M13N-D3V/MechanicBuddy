using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;
using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Infrastructure;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly TenantService _tenantService;
    private readonly ITenantDatabaseProvisioner _dbProvisioner;
    private readonly IKubernetesClient _kubernetesClient;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        TenantService tenantService,
        ITenantDatabaseProvisioner dbProvisioner,
        IKubernetesClient kubernetesClient,
        ILogger<TenantsController> logger)
    {
        _tenantService = tenantService;
        _dbProvisioner = dbProvisioner;
        _kubernetesClient = kubernetesClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var tenants = await _tenantService.GetAllAsync(skip, take);
        return Ok(tenants);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tenant = await _tenantService.GetByIdAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }
        return Ok(tenant);
    }

    [HttpGet("by-tenant-id/{tenantId}")]
    public async Task<IActionResult> GetByTenantId(string tenantId)
    {
        var tenant = await _tenantService.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            return NotFound();
        }
        return Ok(tenant);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        try
        {
            var tenant = await _tenantService.CreateTenantAsync(
                request.CompanyName,
                request.OwnerEmail,
                request.OwnerName,
                request.Tier ?? "free",
                request.IsDemo
            );

            return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tenant");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Tenant tenant)
    {
        if (id != tenant.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var success = await _tenantService.UpdateTenantAsync(tenant);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{tenantId}/suspend")]
    public async Task<IActionResult> Suspend(string tenantId, [FromBody] SuspendTenantRequest request)
    {
        var success = await _tenantService.SuspendTenantAsync(tenantId, request.Reason);
        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "Tenant suspended successfully" });
    }

    [HttpPost("{tenantId}/resume")]
    public async Task<IActionResult> Resume(string tenantId)
    {
        var success = await _tenantService.ResumeTenantAsync(tenantId);
        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "Tenant resumed successfully" });
    }

    [HttpDelete("{tenantId}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> Delete(string tenantId)
    {
        var result = await _tenantService.DeleteTenantAsync(tenantId);

        if (!result.Success)
        {
            // Both K8s and DB deletion failed
            var errorDetails = new List<string>();
            if (!string.IsNullOrEmpty(result.KubernetesError))
                errorDetails.Add($"Kubernetes: {result.KubernetesError}");
            if (!string.IsNullOrEmpty(result.DatabaseError))
                errorDetails.Add($"Database: {result.DatabaseError}");

            var errorMessage = errorDetails.Count > 0
                ? $"Failed to delete tenant: {string.Join("; ", errorDetails)}"
                : "Failed to delete tenant";

            return BadRequest(new
            {
                message = errorMessage,
                kubernetesError = result.KubernetesError,
                databaseError = result.DatabaseError
            });
        }

        // Return details about what was deleted
        return Ok(new
        {
            message = "Tenant deleted successfully",
            kubernetesDeleted = result.KubernetesDeleted,
            databaseDeleted = result.DatabaseDeleted,
            tenantNotInDatabase = result.TenantNotInDatabase,
            warnings = GetDeleteWarnings(result)
        });
    }

    private static List<string> GetDeleteWarnings(Services.DeleteTenantResult result)
    {
        var warnings = new List<string>();

        if (result.TenantNotInDatabase)
        {
            warnings.Add("Tenant was not found in database (orphaned Kubernetes resources were cleaned up)");
        }

        if (!string.IsNullOrEmpty(result.KubernetesError))
        {
            warnings.Add($"Kubernetes cleanup warning: {result.KubernetesError}");
        }

        if (!string.IsNullOrEmpty(result.DatabaseError))
        {
            warnings.Add($"Database cleanup warning: {result.DatabaseError}");
        }

        return warnings;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _tenantService.GetStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Provisions the database for an existing tenant.
    /// Use this to fix tenants that were created without database provisioning.
    /// </summary>
    [HttpPost("{tenantId}/provision-database")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> ProvisionDatabase(string tenantId)
    {
        try
        {
            // Check if tenant exists in the management database
            var tenant = await _tenantService.GetByTenantIdAsync(tenantId);
            if (tenant == null)
            {
                return NotFound(new { message = $"Tenant '{tenantId}' not found" });
            }

            // Check if database already exists
            var exists = await _dbProvisioner.TenantDatabaseExistsAsync(tenantId);
            if (exists)
            {
                return Conflict(new { message = $"Database schema for tenant '{tenantId}' already exists" });
            }

            // Provision the database
            _logger.LogInformation("Provisioning database for existing tenant {TenantId}", tenantId);
            var connectionString = await _dbProvisioner.ProvisionTenantDatabaseAsync(tenantId);

            // Update tenant with connection string
            tenant.DbConnectionString = connectionString;
            await _tenantService.UpdateTenantAsync(tenant);

            return Ok(new
            {
                message = $"Database provisioned successfully for tenant '{tenantId}'",
                connectionString
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision database for tenant {TenantId}", tenantId);
            return BadRequest(new { message = ex.Message });
        }
    }
    /// <summary>
    /// Restarts the API deployment for a tenant.
    /// </summary>
    [HttpPost("{tenantId}/restart-api")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> RestartApi(string tenantId)
    {
        try
        {
            var tenant = await _tenantService.GetByTenantIdAsync(tenantId);
            if (tenant == null)
            {
                return NotFound(new { message = $"Tenant '{tenantId}' not found" });
            }

            _logger.LogInformation("Restarting API deployment for tenant {TenantId}", tenantId);
            await _kubernetesClient.RestartDeploymentAsync(tenantId, "api");

            return Ok(new { message = $"API deployment restarted for tenant '{tenantId}'" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart API deployment for tenant {TenantId}", tenantId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Restarts the frontend deployment for a tenant.
    /// </summary>
    [HttpPost("{tenantId}/restart-frontend")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> RestartFrontend(string tenantId)
    {
        try
        {
            var tenant = await _tenantService.GetByTenantIdAsync(tenantId);
            if (tenant == null)
            {
                return NotFound(new { message = $"Tenant '{tenantId}' not found" });
            }

            _logger.LogInformation("Restarting frontend deployment for tenant {TenantId}", tenantId);
            await _kubernetesClient.RestartDeploymentAsync(tenantId, "frontend");

            return Ok(new { message = $"Frontend deployment restarted for tenant '{tenantId}'" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart frontend deployment for tenant {TenantId}", tenantId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Runs database migrations for a tenant.
    /// </summary>
    [HttpPost("{tenantId}/run-migration")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> RunMigration(string tenantId)
    {
        try
        {
            var tenant = await _tenantService.GetByTenantIdAsync(tenantId);
            if (tenant == null)
            {
                return NotFound(new { message = $"Tenant '{tenantId}' not found" });
            }

            _logger.LogInformation("Running database migration for tenant {TenantId}", tenantId);
            var jobName = await _kubernetesClient.RunMigrationJobAsync(tenantId);

            return Ok(new { message = $"Migration job '{jobName}' started for tenant '{tenantId}'", jobName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run migration for tenant {TenantId}", tenantId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Restarts all active tenant API and frontend deployments.
    /// </summary>
    [HttpPost("restart-all")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> RestartAll()
    {
        try
        {
            var tenants = await _tenantService.GetAllAsync(0, 1000);
            var activeTenants = tenants.Where(t => t.Status == "active").ToList();

            _logger.LogInformation("Restarting all {Count} active tenant deployments", activeTenants.Count);

            var results = new List<object>();
            var errors = new List<object>();

            foreach (var tenant in activeTenants)
            {
                try
                {
                    await _kubernetesClient.RestartDeploymentAsync(tenant.TenantId, "api");
                    await _kubernetesClient.RestartDeploymentAsync(tenant.TenantId, "frontend");
                    results.Add(new { tenantId = tenant.TenantId, success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to restart deployments for tenant {TenantId}", tenant.TenantId);
                    errors.Add(new { tenantId = tenant.TenantId, error = ex.Message });
                }
            }

            return Ok(new
            {
                message = $"Restart initiated for {results.Count} tenants",
                totalTenants = activeTenants.Count,
                successCount = results.Count,
                errorCount = errors.Count,
                results,
                errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart all tenant deployments");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Runs database migrations for all active tenants.
    /// </summary>
    [HttpPost("migrate-all")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> MigrateAll()
    {
        try
        {
            var tenants = await _tenantService.GetAllAsync(0, 1000);
            var activeTenants = tenants.Where(t => t.Status == "active").ToList();

            _logger.LogInformation("Running migrations for all {Count} active tenants", activeTenants.Count);

            var results = new List<object>();
            var errors = new List<object>();

            foreach (var tenant in activeTenants)
            {
                try
                {
                    var jobName = await _kubernetesClient.RunMigrationJobAsync(tenant.TenantId);
                    results.Add(new { tenantId = tenant.TenantId, jobName, success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to run migration for tenant {TenantId}", tenant.TenantId);
                    errors.Add(new { tenantId = tenant.TenantId, error = ex.Message });
                }
            }

            return Ok(new
            {
                message = $"Migration jobs started for {results.Count} tenants",
                totalTenants = activeTenants.Count,
                successCount = results.Count,
                errorCount = errors.Count,
                results,
                errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run migrations for all tenants");
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record CreateTenantRequest(string CompanyName, string OwnerEmail, string OwnerName, string? Tier, bool IsDemo);
public record SuspendTenantRequest(string Reason);
