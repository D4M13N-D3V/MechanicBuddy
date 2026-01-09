using MechanicBuddy.Management.Api.Repositories;

namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Background service for suspending inactive free tier tenants.
/// Runs periodically to check for tenants with no activity for 7+ days.
/// </summary>
public class InactivityCheckService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InactivityCheckService> _logger;
    private readonly IConfiguration _configuration;
    private Timer? _checkTimer;

    private const int DefaultInactivityDays = 7;
    private const int DefaultCheckIntervalHours = 1;

    public InactivityCheckService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<InactivityCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inactivity Check Service is starting");

        var checkIntervalHours = _configuration.GetValue("InactivityCheck:IntervalHours", DefaultCheckIntervalHours);

        // Run inactivity check periodically (default: every hour)
        _checkTimer = new Timer(
            DoInactivityCheck,
            null,
            TimeSpan.FromMinutes(10), // Start after 10 minutes (allow system to stabilize)
            TimeSpan.FromHours(checkIntervalHours));

        return Task.CompletedTask;
    }

    private async void DoInactivityCheck(object? state)
    {
        _logger.LogInformation("Starting inactivity check task");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var tenantRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
            var tenantService = scope.ServiceProvider.GetRequiredService<TenantService>();

            var inactivityDays = _configuration.GetValue("InactivityCheck:InactivityDays", DefaultInactivityDays);

            // Get inactive free tier tenants (excludes lifetime by query)
            var inactiveTenants = await tenantRepository.GetInactiveFreeTierTenantsAsync(inactivityDays);
            var suspendedCount = 0;

            foreach (var tenant in inactiveTenants)
            {
                // Double-check: Skip lifetime tier (belt and suspenders)
                if (tenant.Tier?.ToLower() == "lifetime")
                {
                    _logger.LogDebug("Skipping lifetime tenant {TenantId} from inactivity check", tenant.TenantId);
                    continue;
                }

                try
                {
                    var reason = $"Auto-suspended due to {inactivityDays} days of inactivity on free tier";
                    var success = await tenantService.SuspendTenantAsync(tenant.TenantId, reason);

                    if (success)
                    {
                        suspendedCount++;
                        _logger.LogInformation(
                            "Suspended inactive tenant {TenantId} (Tier: {Tier}, Last Activity: {LastActivity})",
                            tenant.TenantId,
                            tenant.Tier,
                            tenant.LastActivityAt?.ToString("o") ?? "never");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to suspend inactive tenant {TenantId}", tenant.TenantId);
                }
            }

            _logger.LogInformation("Inactivity check completed. Suspended {Count} inactive tenants", suspendedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during inactivity check");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inactivity Check Service is stopping");
        _checkTimer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _checkTimer?.Dispose();
    }
}
