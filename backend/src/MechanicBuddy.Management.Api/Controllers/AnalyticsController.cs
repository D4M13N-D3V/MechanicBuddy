using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(AnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetPlatformOverview()
    {
        var overview = await _analyticsService.GetPlatformOverviewAsync();
        return Ok(overview);
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetTenantMetrics(string tenantId, [FromQuery] int days = 30)
    {
        var metrics = await _analyticsService.GetTenantMetricsAsync(tenantId, days);
        return Ok(metrics);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var analytics = await _analyticsService.GetRevenueAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("top-tenants")]
    public async Task<IActionResult> GetTopTenants([FromQuery] string metric = "work_orders", [FromQuery] int limit = 10)
    {
        var topTenants = await _analyticsService.GetTopTenantsByMetricAsync(metric, limit);
        return Ok(topTenants);
    }
}
