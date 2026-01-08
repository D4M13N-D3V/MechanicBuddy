using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;

namespace MechanicBuddy.Management.Api.Controllers;

/// <summary>
/// Controller for viewing audit logs. Only accessible by super admin users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IAuditLogRepository repository,
        ILogger<AuditLogsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated audit logs with filtering options.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPage(
        [FromQuery] string? searchText,
        [FromQuery] string? actionType,
        [FromQuery] string? tenantId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            var items = await _repository.GetPageAsync(
                searchText, actionType, tenantId, fromDate, toDate, limit, offset);
            var total = await _repository.GetCountAsync(
                searchText, actionType, tenantId, fromDate, toDate);

            return Ok(new AuditLogPageResult
            {
                Items = items.ToArray(),
                Total = total,
                HasMore = (offset + limit) < total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit logs");
            return StatusCode(500, "An error occurred while fetching audit logs");
        }
    }

    /// <summary>
    /// Get audit log statistics for the last N days.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] int days = 7)
    {
        try
        {
            var stats = await _repository.GetStatsAsync(days);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit log stats");
            return StatusCode(500, "An error occurred while fetching audit log stats");
        }
    }
}

/// <summary>
/// Paginated result for audit logs.
/// </summary>
public class AuditLogPageResult
{
    public AuditLog[] Items { get; set; } = Array.Empty<AuditLog>();
    public int Total { get; set; }
    public bool HasMore { get; set; }
}
