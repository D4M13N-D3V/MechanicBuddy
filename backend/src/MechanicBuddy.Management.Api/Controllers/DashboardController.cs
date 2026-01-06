using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;
using MechanicBuddy.Management.Api.Repositories;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IDemoRequestRepository _demoRequestRepository;
    private readonly IBillingEventRepository _billingEventRepository;
    private readonly ITenantMetricsRepository _metricsRepository;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ITenantRepository tenantRepository,
        IDemoRequestRepository demoRequestRepository,
        IBillingEventRepository billingEventRepository,
        ITenantMetricsRepository metricsRepository,
        ILogger<DashboardController> logger)
    {
        _tenantRepository = tenantRepository;
        _demoRequestRepository = demoRequestRepository;
        _billingEventRepository = billingEventRepository;
        _metricsRepository = metricsRepository;
        _logger = logger;
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetDashboardAnalytics()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        // Get tenant counts
        var statusCounts = await _tenantRepository.GetCountByStatusAsync();
        var totalTenants = await _tenantRepository.GetTotalCountAsync();
        var tierCounts = await _tenantRepository.GetCountByTierAsync();

        // Get revenue data
        var totalRevenue = await _billingEventRepository.GetTotalRevenueAsync();
        var monthlyRevenue = await _billingEventRepository.GetTotalRevenueAsync(monthStart, now);

        // Get demo request stats
        var pendingDemos = await _demoRequestRepository.GetCountByStatusAsync("pending");
        var totalDemos = await _demoRequestRepository.GetTotalCountAsync();
        var convertedDemos = await _demoRequestRepository.GetCountByStatusAsync("converted");

        // Get recent tenants
        var recentTenants = await _tenantRepository.GetAllAsync(0, 5);

        // Calculate revenue by month (last 6 months)
        var revenueByMonth = new List<object>();
        for (int i = 5; i >= 0; i--)
        {
            var monthDate = now.AddMonths(-i);
            var start = new DateTime(monthDate.Year, monthDate.Month, 1);
            var end = start.AddMonths(1);
            var revenue = await _billingEventRepository.GetTotalRevenueAsync(start, end);
            var tenantCount = await _tenantRepository.GetCountCreatedBetweenAsync(start, end);

            revenueByMonth.Add(new
            {
                month = start.ToString("MMM"),
                revenue = revenue,
                tenants = tenantCount
            });
        }

        // Build plan distribution
        var tenantsByPlan = tierCounts.Select(kvp => new
        {
            plan = kvp.Key,
            count = kvp.Value,
            revenue = GetRevenueByTier(kvp.Key, kvp.Value)
        }).ToList();

        var activeTenants = statusCounts.GetValueOrDefault("active", 0);
        var trialTenants = statusCounts.GetValueOrDefault("trial", 0);
        var suspendedTenants = statusCounts.GetValueOrDefault("suspended", 0);

        var conversionRate = totalDemos > 0
            ? Math.Round((decimal)convertedDemos / totalDemos * 100, 2)
            : 0;

        var averageRevenue = activeTenants > 0
            ? Math.Round(monthlyRevenue / activeTenants, 2)
            : 0;

        return Ok(new
        {
            totalTenants,
            activeTenants,
            trialTenants,
            suspendedTenants,
            totalRevenue,
            monthlyRecurringRevenue = monthlyRevenue,
            averageRevenuePerTenant = averageRevenue,
            totalDemoRequests = totalDemos,
            pendingDemoRequests = pendingDemos,
            conversionRate,
            recentTenants = recentTenants.Select(t => new
            {
                id = t.Id.ToString(),
                name = t.CompanyName,
                plan = t.Tier,
                status = t.Status,
                joinedAt = t.CreatedAt.ToString("yyyy-MM-dd")
            }),
            revenueByMonth,
            tenantsByPlan
        });
    }

    private static decimal GetRevenueByTier(string tier, int count)
    {
        // Approximate revenue based on tier pricing
        return tier switch
        {
            "free" => 0,
            "starter" => count * 20m,
            "professional" => count * 15m * 15, // Avg 15 mechanics
            "enterprise" => count * 10m * 30,   // Avg 30 mechanics
            _ => 0
        };
    }
}
