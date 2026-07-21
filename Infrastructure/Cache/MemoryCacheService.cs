using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;


namespace Infrastructure.Cache
{

    // Ylopoiei to ICacheService panw sto IMemoryCache tou .NET.
    // Prosferei get / set / remove panw sto in-memory cache tis efarmogis.
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        // Pairnoume to IMemoryCache meso DI kai to kratame gia na to xrisimopoioume parakato.
        public MemoryCacheService(IMemoryCache cache) => _memoryCache = cache;

       


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
            // An to ttl einai null, i eggrafi den lixei pote.

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
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
