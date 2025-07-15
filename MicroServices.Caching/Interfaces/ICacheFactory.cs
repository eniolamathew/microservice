using Microsoft.Extensions.Caching.Memory;

namespace MicroServices.Caching.Interfaces
{
    public interface ICacheFactory
    {
        IMicroserviceCache<TCacheEntity> CreateCache<TCacheEntity>(
            string cacheName,
            TimeSpan? expiration = null) where TCacheEntity : class;

        IMemoryCache GetMemoryCache(string cacheName);
        void ResetCache(string cacheName);
    }
}