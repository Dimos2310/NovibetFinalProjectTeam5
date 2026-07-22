using Application.Interfaces;
using Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;


namespace Infrastructure.Cache
{

    // Ylopoiei to ICacheService panw sto IMemoryCache tou .NET.
    // Prosferei get / set / remove panw sto in-memory cache tis efarmogis.
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _defaultTtl;

        // Pairnoume to IMemoryCache meso DI kai to kratame gia na to xrisimopoioume parakato.
        // Pairnoume kai to CacheOptions gia na exoume ena default TTL otan o caller den
        // dosei o idios ena (px to IpInfoService.SetAsync den perase ttl) - alliws to
        // "Cache:TtlMinutes" sto appsettings tha itan mia rythmisi pou kaneis den diavazei.
        public MemoryCacheService(IMemoryCache cache, IOptions<CacheOptions> cacheOptions)
        {
            _memoryCache = cache;
            _defaultTtl = TimeSpan.FromMinutes(cacheOptions.Value.TtlMinutes);
        }

       

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {

            // Psaxnoume sto cache tin timi pou antistoixei sto key.
            // An vrethei, gemizei to value. An den yparxei, to value pairnei default.
            _memoryCache.TryGetValue(key, out T?  value);

            // Epistrefoume tin timi se Task, giati to interface einai async.
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {

            // Ftiaxnoume tis rythmiseis tis eggrafis kai orizoume ttl.
            // An o caller den dosei diko tou ttl, xrisimopoioume to default apo to
            // appsettings ("Cache:TtlMinutes") anti na apothikevoume gia panta.

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
            };

            // Apothikevoume tin timi sto cache me to sygkekrimeno key

            _memoryCache.Set(key, value, options);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {

            // Diagrafoume tin eggrafi tou key apo to cache (gia invalidation apo to update job).
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

      
    }
}
