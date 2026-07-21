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
public sealed class IpRefreshBackgroundService : BackgroundService // backfoundservice Είναι μια έτοιμη βάση από το .NET για «υπηρεσίες που τρέχουν στο παρασκήνιο όσο ζει η εφαρμογή
                                                                   // Της δίνεις μία μέθοδο, την ExecuteAsync, και το framework τη φωνάζει αυτόματα στο ξεκίνημα της εφαρμογής.
                                                                   // Δεν χρειάζεται εσύ να τη «ξεκινήσεις»
{
    //parametroi
    private readonly IServiceScopeFactory _scopeFactory;  // για να φτιάχνω φρέσκα DI scopes ανά εκτέλεση, ώστε να παίρνω σωστά-χρονισμένα IIpRefreshService
    private readonly IpRefreshOptions _options;           // για να ξέρω πόσο συχνά να τρέχω και πόσα IPs να παίρνω ανά batch
    private readonly ILogger<IpRefreshBackgroundService> _logger;  // για να log-άρω σφάλματα και πληροφορίες

    // constructor
    public IpRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<IpRefreshOptions> options,
        ILogger<IpRefreshBackgroundService> logger)
    {
        // private fields gai na ta xrisimopoiei meta
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

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
            }
        }
    }

    // Κάθε εκτέλεση του job. Ανοίγει φρέσκο DI scope, παίρνει IIpRefreshService και το καλεί. Αν αποτύχει, log-άρει και συνεχίζει.
    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
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