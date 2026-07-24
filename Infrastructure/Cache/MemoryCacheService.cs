<<<<<<< HEAD
using Application.Interfaces;
=======
﻿using Application.Interfaces;
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
using Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

<<<<<<< HEAD
namespace Infrastructure.Cache
{
    // Implements ICacheService on top of .NET's built-in IMemoryCache.
    // Provides get / set / remove over the app's in-memory cache.
=======

namespace Infrastructure.Cache
{

    // Ylopoiei to ICacheService panw sto IMemoryCache tou .NET.
    // Prosferei get / set / remove panw sto in-memory cache tis efarmogis.
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _defaultTtl;

<<<<<<< HEAD
        // IMemoryCache comes from DI (registered via services.AddMemoryCache()).
        // CacheOptions is also injected so there's a sensible default TTL for callers that
        // don't pass one themselves (e.g. IpInfoService's SetAsync calls) - otherwise
        // "Cache:TtlMinutes" in appsettings would be a setting nobody actually reads.
=======
        // Pairnoume to IMemoryCache meso DI kai to kratame gia na to xrisimopoioume parakato.
        // Pairnoume kai to CacheOptions gia na exoume ena default TTL otan o caller den
        // dosei o idios ena (px to IpInfoService.SetAsync den perase ttl) - alliws to
        // "Cache:TtlMinutes" sto appsettings tha itan mia rythmisi pou kaneis den diavazei.
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
        public MemoryCacheService(IMemoryCache cache, IOptions<CacheOptions> cacheOptions)
        {
            _memoryCache = cache;
            _defaultTtl = TimeSpan.FromMinutes(cacheOptions.Value.TtlMinutes);
        }

<<<<<<< HEAD
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            // Look up the value for this key. If found, "value" is populated; if not,
            // it's left as default(T).
            _memoryCache.TryGetValue(key, out T? value);

            // Wrapped in a Task because ICacheService's contract is async (so a future
            // Redis-backed implementation could swap in without changing the interface).
=======
       

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {

            // Psaxnoume sto cache tin timi pou antistoixei sto key.
            // An vrethei, gemizei to value. An den yparxei, to value pairnei default.
            _memoryCache.TryGetValue(key, out T?  value);

            // Epistrefoume tin timi se Task, giati to interface einai async.
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
<<<<<<< HEAD
            // If the caller didn't pass their own ttl, fall back to the configured default
            // from appsettings ("Cache:TtlMinutes") instead of caching forever.
=======

            // Ftiaxnoume tis rythmiseis tis eggrafis kai orizoume ttl.
            // An o caller den dosei diko tou ttl, xrisimopoioume to default apo to
            // appsettings ("Cache:TtlMinutes") anti na apothikevoume gia panta.

>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
            };

<<<<<<< HEAD
            _memoryCache.Set(key, value, options);
=======
            // Apothikevoume tin timi sto cache me to sygkekrimeno key

            _memoryCache.Set(key, value, options);

>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
<<<<<<< HEAD
            // Used by the refresh job (Task 2) to invalidate a cached entry once its
            // country data has actually changed.
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
=======

            // Diagrafoume tin eggrafi tou key apo to cache (gia invalidation apo to update job).
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

      
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
    }
}
