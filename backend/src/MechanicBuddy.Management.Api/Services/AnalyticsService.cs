using MechanicBuddy.Management.Api.Repositories;

namespace MechanicBuddy.Management.Api.Services;

public class AnalyticsService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantMetricsRepository _metricsRepository;
    private readonly IBillingEventRepository _billingEventRepository;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        ITenantRepository tenantRepository,
        ITenantMetricsRepository metricsRepository,
        IBillingEventRepository billingEventRepository,
        ILogger<AnalyticsService> logger)
    {
        _tenantRepository = tenantRepository;
        _metricsRepository = metricsRepository;
        _billingEventRepository = billingEventRepository;
        _logger = logger;
    }

    public async Task<Dictionary<string, object>> GetPlatformOverviewAsync()
    {
        var tenantStats = await _tenantRepository.GetCountByStatusAsync();
        var tierStats = await _tenantRepository.GetCountByTierAsync();
        var totalTenants = await _tenantRepository.GetTotalCountAsync();
        var aggregateMetrics = await _metricsRepository.GetAggregateMetricsAsync();

        var now = DateTime.UtcNow;
        var monthlyRevenue = await _billingEventRepository.GetTotalRevenueAsync(
            now.AddMonths(-1),
            now
        );
        var totalRevenue = await _billingEventRepository.GetTotalRevenueAsync();

        return new Dictionary<string, object>
        {
            ["totalTenants"] = totalTenants,
            ["tenantsByStatus"] = tenantStats,
            ["tenantsByTier"] = tierStats,
            ["metrics"] = aggregateMetrics,
            ["revenue"] = new Dictionary<string, object>
            {
                ["monthly"] = monthlyRevenue,
                ["total"] = totalRevenue
            }
        };
    }

    public async Task<Dictionary<string, object>> GetTenantMetricsAsync(string tenantId, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var metrics = await _metricsRepository.GetByTenantIdAsync(tenantId, startDate);
        var latest = await _metricsRepository.GetLatestByTenantIdAsync(tenantId);

        return new Dictionary<string, object>
        {
            ["tenantId"] = tenantId,
            ["period"] = $"{days} days",
            ["latest"] = latest ?? new object(),
            ["history"] = metrics.OrderBy(m => m.RecordedAt).ToList()
        };
    }

    public async Task<Dictionary<string, object>> GetRevenueAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var totalRevenue = await _billingEventRepository.GetTotalRevenueAsync(startDate, endDate);

        // Calculate MRR (Monthly Recurring Revenue)
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var currentMonthRevenue = await _billingEventRepository.GetTotalRevenueAsync(currentMonthStart, now);

        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var lastMonthRevenue = await _billingEventRepository.GetTotalRevenueAsync(lastMonthStart, currentMonthStart);

        var growthRate = lastMonthRevenue > 0
            ? ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
            : 0;

        return new Dictionary<string, object>
        {
            ["totalRevenue"] = totalRevenue,
            ["currentMonthRevenue"] = currentMonthRevenue,
            ["lastMonthRevenue"] = lastMonthRevenue,
            ["growthRate"] = Math.Round(growthRate, 2),
            ["period"] = new
            {
                start = startDate?.ToString("yyyy-MM-dd") ?? "all time",
                end = endDate?.ToString("yyyy-MM-dd") ?? "now"
            }
        };
    }

    public async Task<List<Dictionary<string, object>>> GetTopTenantsByMetricAsync(string metric, int limit = 10)
    {
        // This is a simplified version - in production, you'd want more sophisticated queries
        var allTenants = await _tenantRepository.GetAllAsync(0, 1000);
        var results = new List<Dictionary<string, object>>();

        foreach (var tenant in allTenants)
        {
            var latestMetrics = await _metricsRepository.GetLatestByTenantIdAsync(tenant.TenantId);
            if (latestMetrics != null)
            {
                var value = metric switch
                {
                    "work_orders" => latestMetrics.WorkOrdersCount,
                    "clients" => latestMetrics.ClientsCount,
                    "vehicles" => latestMetrics.VehiclesCount,
                    "storage" => latestMetrics.StorageUsed,
                    "api_calls" => latestMetrics.ApiCallsCount,
                    _ => 0
                };

                results.Add(new Dictionary<string, object>
                {
                    ["tenantId"] = tenant.TenantId,
                    ["companyName"] = tenant.CompanyName,
                    ["tier"] = tenant.Tier,
                    ["value"] = value
                });
            }
        }

        return results
            .OrderByDescending(r => Convert.ToInt64(r["value"]))
            .Take(limit)
            .ToList();
    }
}
