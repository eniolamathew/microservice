using Microsoft.Extensions.Caching.Memory;
using MicroServices.Caching.Implementations;
using MicroServices.Caching.Interfaces;
using MicroServices.Caching.ServiceCaches;

namespace MicroServices.Caching.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMicroserviceCaching(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheFactory, CacheFactory>();
            services.AddScoped<IPlatformCache, PlatformCache>();
            services.AddTransient(typeof(IMicroserviceCache<>), typeof(MicroserviceCache<>));
            return services;
        }

        public static IServiceCollection AddMicroserviceCaching(this IServiceCollection services,
            Action<MemoryCacheOptions> configureOptions)
        {
            services.AddMemoryCache(configureOptions);
            services.AddSingleton<ICacheFactory, CacheFactory>();
            services.AddTransient(typeof(IMicroserviceCache<>), typeof(MicroserviceCache<>));
            return services;
        }
    }
}
