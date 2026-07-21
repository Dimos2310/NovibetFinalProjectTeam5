namespace Application.BackgroundJobs;

/// <summary>
/// Entry point for the hourly IP refresh job (Task 2). Its single capability is to
/// re-check every stored IP against IP2C and bring the database and cache up to date.
/// The scheduler depends on this abstraction, not the concrete class, so the "how"
/// (batching, comparison, caching) can change without touching the timer or the tests.
/// </summary>
public interface IIpRefreshService
{
    /// That's the whole "refresh everything" promise. The scheduler (Step 7) will call only this and know nothing else about your logic
    /// A plain Task is right for a fire-and-run operation. Gt den gurnaei kati pisw gia na einai Task<kati>
    /// The token lets a run stop cleanly on app shutdown. The background service will pass its stoppingToken in here, 
    /// and it flows down into every DB/IP2C/cache call so the whole chain can bail out promptly.
    Task RefreshAllAsync(CancellationToken cancellationToken = default);
}