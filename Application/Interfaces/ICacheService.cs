namespace Application.Interfaces;

// Abstraction over caching, so the implementation (in-memory today) can be swapped
// for something distributed (e.g. Redis) without touching Task 1 or Task 2.
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
