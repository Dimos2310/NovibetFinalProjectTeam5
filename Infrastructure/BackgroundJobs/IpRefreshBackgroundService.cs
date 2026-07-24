using Application.BackgroundJobs;   // IpRefreshOptions, IIpRefreshService
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Φιλοξενεί την ωριαία ανανέωση (Task 2). Αυτή η κλάση αποφασίζει ΜΟΝΟ το *πότε*: κρατάει
/// τον timer, ανοίγει φρέσκο DI scope ανά εκτέλεση, και καλεί το IIpRefreshService, που κρατάει
/// το *πώς*. Χωρίζοντας χρονισμό και λογική, η λογική τεστάρεται χωρίς ρολόι.
/// </summary>
public sealed class IpRefreshBackgroundService : BackgroundService
    // BackgroundService is a base class from .NET for "services that run in the background
    // for as long as the app is alive". You give it one method (ExecuteAsync) and the
    // framework calls it automatically on startup - nothing here has to "start" it manually.
{
    private readonly IServiceScopeFactory _scopeFactory; // creates a fresh DI scope per run, so each run gets a correctly-scoped IIpRefreshService
    private readonly IpRefreshOptions _options;          // how often to run, and how many IPs per batch
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

    // "protected" because BackgroundService declares it that way; "override" because it's
    // the one virtual method the framework calls for us - we never call this directly.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Fires once immediately, then again every "Interval" (default 1 hour, see
        // IpRefreshOptions). "using" because PeriodicTimer is IDisposable.
        using var timer = new PeriodicTimer(_options.Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Each run is independent, with its own DI scope and its own cancellation
            // token. If a run takes longer than one Interval, the next run simply waits
            // for the timer's next tick rather than overlapping with the one still going.
            await RunOnceAsync(stoppingToken);

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break; // the timer itself was disposed
            }
            catch (OperationCanceledException)
            {
                break; // the app is shutting down
            }
        }
    }

    // One execution of the job: opens a fresh DI scope, resolves IIpRefreshService, and
    // runs it. Any failure is logged and swallowed here - see the catch below for why.
    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Fresh scope per run: this job itself is a singleton (registered once, lives
            // for the app's whole lifetime), but the repository/DbContext it depends on
            // are scoped. Creating a scope here gives us a correctly-lived IIpRefreshService
            // for just this one run, and disposing the scope releases its DB connection.
            using var scope = _scopeFactory.CreateScope();
            var refresher = scope.ServiceProvider.GetRequiredService<IIpRefreshService>();
            await refresher.RefreshAllAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected when the app is shutting down mid-run - not a real error, just stop.
        }
        catch (Exception ex)
        {
            // One failed run must NOT kill the whole loop - log it and let the next tick try again.
            _logger.LogError(ex, "IP refresh run failed; will retry on the next tick.");
        }
    }
}
