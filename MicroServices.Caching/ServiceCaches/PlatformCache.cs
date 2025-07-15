using AutoMapper;
using MicroServices.Caching.Interfaces;
using MicroServices.Caching.Model.Entities;

namespace MicroServices.Caching.ServiceCaches
{
    public class PlatformCache : IPlatformCache
    {
        private readonly IMapper _mapper;
        private readonly IMicroserviceCache<PlatformCacheEntity> _cache;
        private const string CachePrefix = "platform";

        public PlatformCache(IMapper mapper, ICacheFactory cacheFactory)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cacheFactory.CreateCache<PlatformCacheEntity>("PlatformCache", TimeSpan.FromHours(1));
        }

        public async Task<PlatformCacheEntity> GetPlatformAsync(int id)
        {
            var cacheKey = $"{CachePrefix}_{id}";
            return await _cache.GetAsync(cacheKey);
        }

        public async Task<IEnumerable<PlatformCacheEntity>> GetAllPlatformsAsync()
        {
            var cachedPlatforms = await _cache.GetAllAsync();
            return cachedPlatforms as IEnumerable<PlatformCacheEntity> ?? Enumerable.Empty<PlatformCacheEntity>();
        }

        public async Task AddOrUpdatePlatformAsync(PlatformCacheEntity platform)
        {
            if (platform == null) return;

            var cacheKey = $"{CachePrefix}_{platform.Id}";
            await _cache.AddOrUpdateAsync(cacheKey, platform);
            await _cache.RemoveAllAsync();
        }

        public async Task RemovePlatformAsync(int id)
        {
            await _cache.RemoveAsync($"{CachePrefix}_{id}");
            await _cache.RemoveAllAsync();
        }

        public async Task ClearAllPlatformsAsync()
        {
            await _cache.ClearAsync();
        }

        public async Task SetAllPlatformsAsync(IEnumerable<PlatformCacheEntity> platforms)
        {
            if (platforms == null) return;

            foreach (var platform in platforms)
            {
                await _cache.AddOrUpdateAsync($"{CachePrefix}_{platform.Id}", platform);
            }

            await _cache.AddOrUpdateBulkAsync($"{CachePrefix}_all", platforms);
        }

        public async Task<IEnumerable<PlatformCacheEntity>> GetPlatformsByIdsAsync(IEnumerable<int> ids)
        {
            var platforms = new List<PlatformCacheEntity>();

            foreach (var id in ids)
            {
                var platform = await _cache.GetAsync($"{CachePrefix}_{id}");
                if (platform != null)
                    platforms.Add(platform);
            }

            return platforms;
        }

        public async Task<bool> PlatformExistsAsync(int id)
        {
            var platform = await GetPlatformAsync(id);
            return platform != null;
        }

    }
}



//using AutoMapper;
//using MicroServices.API.Interfaces;
//using MicroServices.Caching.Interfaces;
//using MicroServices.Caching.Model.Entities;


//namespace MicroServices.Caching.ServiceCaches
//{
//    public class PlatformCache : IPlatformCache
//    {
//        private readonly IPlatformApi _platformApi;

//        private readonly IMapper _mapper;
//        private readonly IMicroserviceCache<PlatformCacheEntity> _cache;
//        private const string CachePrefix = "platform";

//        public PlatformCache(IPlatformApi platformApi, IMapper mapper, ICacheFactory cacheFactory)
//        {
//            _platformApi = platformApi ?? throw new ArgumentNullException(nameof(platformApi));
//            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

//            _cache = cacheFactory.CreateCache<PlatformCacheEntity>("PlatformCache",TimeSpan.FromHours(1));
//        }

//        public async Task<PlatformCacheEntity> GetPlatformAsync(int id)
//        {
//            var cacheKey = $"{CachePrefix}_{id}";
//            var cachedPlatform = await _cache.GetAsync(cacheKey);

//            if (cachedPlatform != null)
//                return cachedPlatform;

//            var apiResponse = await _platformApi.GetByIdAsync(id);
//            if (apiResponse?.Payload == null)
//                return null;

//            var platformEntity = _mapper.Map<PlatformCacheEntity>(apiResponse.Payload);
//            await _cache.AddOrUpdateAsync(cacheKey, platformEntity);

//            return platformEntity;
//        }

//        public async Task<IEnumerable<PlatformCacheEntity>> GetAllPlatformsAsync()
//        {

//             var allPlatformsKey = $"{CachePrefix}_all";
//            var cachedPlatforms = await _cache.GetAsync(allPlatformsKey);

//            if (cachedPlatforms is IEnumerable<PlatformCacheEntity> platforms)
//            {
//                return platforms;
//            }

//            var apiResponse = await _platformApi.GetAllAsync();
//            if (apiResponse?.Payload == null)
//                return Enumerable.Empty<PlatformCacheEntity>();

//            var platformsFromApi = _mapper.Map<IEnumerable<PlatformCacheEntity>>(apiResponse.Payload);

//            foreach (var platform in platformsFromApi)
//            {
//                await _cache.AddOrUpdateAsync($"{CachePrefix}_{platform.Id}", platform);
//        }

//            await _cache.AddOrUpdateBulkAsync(allPlatformsKey, platformsFromApi);

//            return platformsFromApi;
//        }

//        public async Task AddOrUpdatePlatformAsync(PlatformCacheEntity platform)
//        {
//            if (platform == null) return;

//            var cacheKey = $"{CachePrefix}_{platform.Id}";
//            await _cache.AddOrUpdateAsync(cacheKey, platform);
//            await _cache.RemoveAsync($"{CachePrefix}_all");
//        }

//        public async Task RemovePlatformAsync(int id)
//        {
//            await _cache.RemoveAsync($"{CachePrefix}_{id}");
//            await _cache.RemoveAsync($"{CachePrefix}_all");
//        }

//        public async Task RefreshPlatformAsync(int id)
//        {
//            var apiResponse = await _platformApi.GetByIdAsync(id);
//            if (apiResponse?.Payload != null)
//            {
//                var platformEntity = _mapper.Map<PlatformCacheEntity>(apiResponse.Payload);
//                await AddOrUpdatePlatformAsync(platformEntity);
//            }
//        }

//        public async Task ClearAllPlatformsAsync()
//        {
//            await _cache.ClearAsync();
//        }

//        public async Task SetAllPlatformsAsync(IEnumerable<PlatformCacheEntity> platforms)
//        {
//            if (platforms == null) return;

//            foreach (var platform in platforms)
//            {
//                await _cache.AddOrUpdateAsync($"{CachePrefix}_{platform.Id}", platform);
//            }

//            await _cache.AddOrUpdateBulkAsync($"{CachePrefix}_all", platforms);
//        }

//        public async Task<IEnumerable<PlatformCacheEntity>> GetPlatformsByIdsAsync(IEnumerable<int> ids)
//        {
//            var platforms = new List<PlatformCacheEntity>();
//            var missingIds = new List<int>();

//            foreach (var id in ids)
//            {
//                var platform = await _cache.GetAsync($"{CachePrefix}_{id}");
//                if (platform != null)
//                    platforms.Add(platform);
//                else
//                    missingIds.Add(id);
//            }

//            if (missingIds.Any())
//            {
//                var apiResponse = await _platformApi.GetByIdsAsync(missingIds);
//                if (apiResponse?.Payload != null)
//                {
//                    var freshPlatforms = _mapper.Map<IEnumerable<PlatformCacheEntity>>(apiResponse.Payload);
//                    foreach (var platform in freshPlatforms)
//                    {
//                        await _cache.AddOrUpdateAsync($"{CachePrefix}_{platform.Id}", platform);
//                        platforms.Add(platform);
//                    }
//                }
//            }

//            return platforms;
//        }

//        public async Task<bool> PlatformExistsAsync(int id)
//        {
//            var platform = await GetPlatformAsync(id);
//            return platform != null;
//        }
//    }
//}
