<<<<<<< HEAD
using Application.BackgroundJobs;   // IpRefreshOptions, IIpRefreshService
=======
﻿using Application.BackgroundJobs;   // IpRefreshOptions, IIpRefreshService
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
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
<<<<<<< HEAD
public sealed class IpRefreshBackgroundService : BackgroundService
    // BackgroundService is a base class from .NET for "services that run in the background
    // for as long as the app is alive". You give it one method (ExecuteAsync) and the
    // framework calls it automatically on startup - nothing here has to "start" it manually.
{
    private readonly IServiceScopeFactory _scopeFactory; // creates a fresh DI scope per run, so each run gets a correctly-scoped IIpRefreshService
    private readonly IpRefreshOptions _options;          // how often to run, and how many IPs per batch
    private readonly ILogger<IpRefreshBackgroundService> _logger;

=======
public sealed class IpRefreshBackgroundService : BackgroundService // backfoundservice Είναι μια έτοιμη βάση από το .NET για «υπηρεσίες που τρέχουν στο παρασκήνιο όσο ζει η εφαρμογή
                                                                   // Της δίνεις μία μέθοδο, την ExecuteAsync, και το framework τη φωνάζει αυτόματα στο ξεκίνημα της εφαρμογής.
                                                                   // Δεν χρειάζεται εσύ να τη «ξεκινήσεις»
{
    //parametroi
    private readonly IServiceScopeFactory _scopeFactory;  // για να φτιάχνω φρέσκα DI scopes ανά εκτέλεση, ώστε να παίρνω σωστά-χρονισμένα IIpRefreshService
    private readonly IpRefreshOptions _options;           // για να ξέρω πόσο συχνά να τρέχω και πόσα IPs να παίρνω ανά batch
    private readonly ILogger<IpRefreshBackgroundService> _logger;  // για να log-άρω σφάλματα και πληροφορίες

    // constructor
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
    public IpRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<IpRefreshOptions> options,
        ILogger<IpRefreshBackgroundService> logger)
    {
<<<<<<< HEAD
=======
        // private fields gai na ta xrisimopoiei meta
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

<<<<<<< HEAD
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
=======
    // methods protected gt einai private alla to framework tha thn kalesi apo panw kai override gt einai virtual method apo to BackgroundService kai tha thn kalesi to framework
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // timer pou tha περιμένει το "Interval" kathe 1 wra. using gt o timer exei IDisposable kai tha ton ksanakanei na diagrafei otan teleiwsei to using
        using var timer = new PeriodicTimer(_options.Interval);

        // Τρέξε μία φορά στο ξεκίνημα, μετά κάθε "Interval" 1 wra
        while (!stoppingToken.IsCancellationRequested)
        {
            // Κάθε εκτέλεση είναι ανεξάρτητη, με δικό της DI scope και δικό της ακυρωτικό token. Αν η εκτέλεση διαρκέσει πάνω από 1 ώρα,
            // η επόμενη εκτέλεση θα περιμένει να τελειώσει η προηγούμενη.
            await RunOnceAsync(stoppingToken);
            // Περιμένει το επόμενο tick. Αν η εφαρμογή κλείνει, ο timer θα ακυρωθεί και θα βγούμε από τον βρόχο.
            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break; // ο timer καταστράφηκε
            }
            catch (OperationCanceledException)
            {
                break; // η εφαρμογή κλείνει
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
            }
        }
    }

<<<<<<< HEAD
    // One execution of the job: opens a fresh DI scope, resolves IIpRefreshService, and
    // runs it. Any failure is logged and swallowed here - see the catch below for why.
=======
    // Κάθε εκτέλεση του job. Ανοίγει φρέσκο DI scope, παίρνει IIpRefreshService και το καλεί. Αν αποτύχει, log-άρει και συνεχίζει.
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
<<<<<<< HEAD
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
=======
            // Φρέσκο scope ανά εκτέλεση: ο job είναι singleton, αλλά repository/DbContext είναι
            // scoped. Έτσι παίρνω ένα σωστά-χρονισμένο IIpRefreshService γι' αυτή τη μία εκτέλεση,
            // και το κλείνω (απελευθερώνοντας τη σύνδεση DB) όταν κλείνει το scope.
            using var scope = _scopeFactory.CreateScope();                                   // φρέσκο DI scope
            var refresher = scope.ServiceProvider.GetRequiredService<IIpRefreshService>();   // παίρνω το IIpRefreshService από το scope
            await refresher.RefreshAllAsync(stoppingToken);                                  // καλώ το RefreshAllAsync με το ακυρωτικό token
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // otan i efarmogi kleinei, to cancellationToken tha exei zhtisei akyrwsi kai tha petaksei OperationCanceledException. Den einai sfalma, apla stamatame. anamenomeno
        }
        catch (Exception ex)
        {
            // Μια αποτυχημένη εκτέλεση ΔΕΝ πρέπει να σκοτώσει τον βρόχο. Log και περίμενε το επόμενο tick.
            _logger.LogError(ex, "IP refresh run failed; will retry on the next tick.");
        }
    }
}
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
