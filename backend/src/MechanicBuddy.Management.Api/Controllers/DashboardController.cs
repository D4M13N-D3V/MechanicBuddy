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

        // Get revenue data from billing events
        var totalRevenue = await _billingEventRepository.GetTotalRevenueAsync();
        var revenueByTier = await _billingEventRepository.GetTotalRevenueByTierAsync();

        // Calculate MRR: team subscriptions * $20/month (matching admin-billing page)
        var teamCount = tierCounts.GetValueOrDefault("team", 0);
        var monthlyRecurringRevenue = teamCount * 20m;

        // Get total subscription months for team (for historical revenue calculation)
        var teamSubscriptionMonths = await _tenantRepository.GetTotalSubscriptionMonthsByTierAsync("team");
        var teamHistoricalRevenue = teamSubscriptionMonths * 20m;

        // Use historical revenue if billing events are empty
        if (revenueByTier.GetValueOrDefault("team", 0) == 0 && teamHistoricalRevenue > 0)
        {
            revenueByTier["team"] = teamHistoricalRevenue;
        }

        // Get demo request stats
        var pendingDemos = await _demoRequestRepository.GetCountByStatusAsync("pending");
        var totalDemos = await _demoRequestRepository.GetTotalCountAsync();
        var convertedDemos = await _demoRequestRepository.GetCountByStatusAsync("converted");

        // Get recent tenants
        var recentTenants = await _tenantRepository.GetAllAsync(0, 5);

        // Calculate revenue by month (last 6 months)
        // Use billing events if available, otherwise calculate from team subscriptions
        var revenueByMonth = new List<object>();
        for (int i = 5; i >= 0; i--)
        {
            var monthDate = now.AddMonths(-i);
            var start = new DateTime(monthDate.Year, monthDate.Month, 1);
            var end = start.AddMonths(1);
            var billingRevenue = await _billingEventRepository.GetTotalRevenueAsync(start, end);
            var tenantCount = await _tenantRepository.GetCountCreatedBetweenAsync(start, end);

            // If no billing events, calculate MRR based on team count for that period
            // For current/past months where team subscribers existed, show $20 per team tenant
            decimal revenue = billingRevenue;
            if (billingRevenue == 0 && teamCount > 0 && start <= now)
            {
                // Show MRR for months where team subscriptions were active
                revenue = teamCount * 20m;
            }

            revenueByMonth.Add(new
            {
                month = start.ToString("MMM"),
                revenue = revenue,
                tenants = tenantCount
            });
        }

        // Build plan distribution with correct revenue:
        // - Lifetime: $250 per tenant
        // - Team: total historical payments from billing events
        // - Free/Solo: $0
        var tenantsByPlan = tierCounts.Select(kvp => new
        {
            plan = kvp.Key,
            count = kvp.Value,
            revenue = GetRevenueByTier(kvp.Key, kvp.Value, revenueByTier)
        }).ToList();

        var activeTenants = statusCounts.GetValueOrDefault("active", 0);
        var trialTenants = statusCounts.GetValueOrDefault("trial", 0);
        var suspendedTenants = statusCounts.GetValueOrDefault("suspended", 0);

        var conversionRate = totalDemos > 0
            ? Math.Round((decimal)convertedDemos / totalDemos * 100, 2)
            : 0;

        var averageRevenue = activeTenants > 0
            ? Math.Round(monthlyRecurringRevenue / activeTenants, 2)
            : 0;

        return Ok(new
        {
            totalTenants,
            activeTenants,
            trialTenants,
            suspendedTenants,
            totalRevenue,
            monthlyRecurringRevenue,
            averageRevenuePerTenant = averageRevenue,
            totalDemoRequests = totalDemos,
            pendingDemoRequests = pendingDemos,
            conversionRate,
            recentTenants = recentTenants.Select(t => new
            {
                id = t.Id.ToString(),
                companyName = t.CompanyName,
                tier = t.Tier,
                status = t.Status,
                createdAt = t.CreatedAt.ToString("yyyy-MM-dd")
            }),
            revenueByMonth,
            tenantsByPlan
        });
    }

    private static decimal GetRevenueByTier(string tier, int count, Dictionary<string, decimal> historicalRevenue)
    {
        // Revenue calculation:
        // - Lifetime: $250 per tenant (one-time purchase)
        // - Team: Historical billing payments (total months paid * $20)
        // - Free/Solo: $0
        return tier?.ToLower() switch
        {
            "free" => 0,
            "solo" => 0,
            "team" => historicalRevenue.GetValueOrDefault("team", 0),
            "lifetime" => count * 250m,
            _ => 0
        };
    }
}
