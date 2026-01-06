namespace MechanicBuddy.Management.Api.Services;

/// <summary>
/// Background service for cleaning up expired demos and sending reminders.
/// </summary>
public class DemoCleanupService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DemoCleanupService> _logger;
    private Timer? _cleanupTimer;
    private Timer? _reminderTimer;

    public DemoCleanupService(
        IServiceProvider serviceProvider,
        ILogger<DemoCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Demo Cleanup Service is starting");

        // Run cleanup every 6 hours
        _cleanupTimer = new Timer(
            DoCleanup,
            null,
            TimeSpan.Zero, // Start immediately
            TimeSpan.FromHours(6));

        // Run reminder checks every 12 hours
        _reminderTimer = new Timer(
            DoReminders,
            null,
            TimeSpan.FromMinutes(5), // Start after 5 minutes
            TimeSpan.FromHours(12));

        return Task.CompletedTask;
    }

    private async void DoCleanup(object? state)
    {
        _logger.LogInformation("Starting demo cleanup task");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var demoRequestService = scope.ServiceProvider.GetRequiredService<DemoRequestService>();

            var cleanedCount = await demoRequestService.CleanupExpiredDemosAsync();

            _logger.LogInformation("Demo cleanup completed. Cleaned {Count} expired demos", cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during demo cleanup");
        }
    }

    private async void DoReminders(object? state)
    {
        _logger.LogInformation("Starting demo expiration reminder task");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var demoRequestService = scope.ServiceProvider.GetRequiredService<DemoRequestService>();

            var sentCount = await demoRequestService.SendExpiringDemoRemindersAsync();

            _logger.LogInformation("Demo reminder task completed. Sent {Count} reminders", sentCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during demo reminder task");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Demo Cleanup Service is stopping");

        _cleanupTimer?.Change(Timeout.Infinite, 0);
        _reminderTimer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        _reminderTimer?.Dispose();
    }
}
