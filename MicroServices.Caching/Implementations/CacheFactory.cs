using MicroServices.Caching.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace MicroServices.Caching.Implementations
{
    public class CacheFactory : ICacheFactory
    {
        private readonly ConcurrentDictionary<string, IMemoryCache> _memoryCaches;
        private readonly IOptions<MemoryCacheOptions> _cacheOptions;

        public CacheFactory(IOptions<MemoryCacheOptions> cacheOptions)
        {
            _cacheOptions = cacheOptions;
            _memoryCaches = new ConcurrentDictionary<string, IMemoryCache>();
        }

        public IMicroserviceCache<TCacheEntity> CreateCache<TCacheEntity>(
            string cacheName,
            TimeSpan? expiration = null) where TCacheEntity : class
        {
            var memoryCache = GetMemoryCache(cacheName);
            return new MicroserviceCache<TCacheEntity>(memoryCache, expiration);
        }

        public IMemoryCache GetMemoryCache(string cacheName)
        {
            return _memoryCaches.GetOrAdd(cacheName,
                name => new MemoryCache(_cacheOptions.Value));
        }

        public void ResetCache(string cacheName)
        {
            if (_memoryCaches.TryRemove(cacheName, out var cache))
            {
                cache.Dispose();
            }
        }
    }
}