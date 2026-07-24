using Application.Interfaces;
using Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Infrastructure.Cache
{
    // Implements ICacheService on top of .NET's built-in IMemoryCache.
    // Provides get / set / remove over the app's in-memory cache.
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _defaultTtl;

        // IMemoryCache comes from DI (registered via services.AddMemoryCache()).
        // CacheOptions is also injected so there's a sensible default TTL for callers that
        // don't pass one themselves (e.g. IpInfoService's SetAsync calls) - otherwise
        // "Cache:TtlMinutes" in appsettings would be a setting nobody actually reads.
        public MemoryCacheService(IMemoryCache cache, IOptions<CacheOptions> cacheOptions)
        {
            _memoryCache = cache;
            _defaultTtl = TimeSpan.FromMinutes(cacheOptions.Value.TtlMinutes);
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            // Look up the value for this key. If found, "value" is populated; if not,
            // it's left as default(T).
            _memoryCache.TryGetValue(key, out T? value);

            // Wrapped in a Task because ICacheService's contract is async (so a future
            // Redis-backed implementation could swap in without changing the interface).
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
            // If the caller didn't pass their own ttl, fall back to the configured default
            // from appsettings ("Cache:TtlMinutes") instead of caching forever.
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
            };

            _memoryCache.Set(key, value, options);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            // Used by the refresh job (Task 2) to invalidate a cached entry once its
            // country data has actually changed.
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
