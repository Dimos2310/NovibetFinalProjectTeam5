namespace Application.Interfaces;

<<<<<<< Updated upstream
/// <summary>
/// Abstraction over the caching layer used by the "cache first" lookup (Task 1) and
/// invalidated by the update job (Task 2). Hides whether the backing store is an
/// in-memory cache or a distributed one (e.g. Redis).
/// </summary>
=======
/// Αφαίρεση πάνω από το επίπεδο caching, που χρησιμοποιείται από την αναζήτηση
/// "cache first" (Task 1) και ακυρώνεται από το job ενημέρωσης (Task 2). Κρύβει το αν
/// από πίσω υπάρχει in-memory cache ή κατανεμημένη (π.χ. Redis).
>>>>>>> Stashed changes
public interface ICacheService
{
    /// <summary>Returns the cached value for the key, or default if it is not present.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Stores a value under the key with an optional time-to-live.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>Removes the value for the key (used to invalidate stale entries).</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
