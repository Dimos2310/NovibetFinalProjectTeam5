using Application.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.BackgroundJobs;

// Hosts the hourly refresh (Task 2). This class owns only the *when* (timer, DI scope
// per run); IIpRefreshService owns the *how*, so the logic is testable without a clock.
public sealed class IpRefreshBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IpRefreshOptions _options;
    private readonly ILogger<IpRefreshBackgroundService> _logger;

    public IpRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<IpRefreshOptions> options,
        ILogger<IpRefreshBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnceAsync(stoppingToken);

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Fresh DI scope per run: this service is a singleton, but the repository
            // it depends on is scoped.
            using var scope = _scopeFactory.CreateScope();
            var refresher = scope.ServiceProvider.GetRequiredService<IIpRefreshService>();
            await refresher.RefreshAllAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IP refresh run failed; will retry on the next tick.");
        }
    }
}
