using AutoMapper;
using MicroServices.Caching.Interfaces;
using MicroServices.Caching.Model.Entities;

namespace MicroServices.Caching.ServiceCaches
{
    public class BrandCache : IBrandCache
    {
        private readonly IMapper _mapper;
        private readonly IMicroserviceCache<BrandCacheEntity> _cache;
        private const string CachePrefix = "Brand";

        public BrandCache(IMapper mapper, ICacheFactory cacheFactory)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cacheFactory.CreateCache<BrandCacheEntity>("BrandCache", TimeSpan.FromHours(1));
        }

        public async Task<BrandCacheEntity> GetBrandAsync(int id)
        {
            var cacheKey = $"{CachePrefix}_{id}";
            return await _cache.GetAsync(cacheKey);
        }

        public async Task<IEnumerable<BrandCacheEntity>> GetAllBrandsAsync()
        {
            var cachedPlatforms = await _cache.GetAllAsync();
            return cachedPlatforms as IEnumerable<BrandCacheEntity> ?? Enumerable.Empty<BrandCacheEntity>();
        }

        public async Task AddOrUpdateBrandAsync(BrandCacheEntity brand)
        {
            if (brand == null) return;

            var cacheKey = $"{CachePrefix}_{brand.Id}";
            await _cache.AddOrUpdateAsync(cacheKey, brand);
            await _cache.RemoveAllAsync();
        }

        public async Task RemoveBrandAsync(int id)
        {
            await _cache.RemoveAsync($"{CachePrefix}_{id}");
            await _cache.RemoveAllAsync();
        }

        public async Task ClearAllBrandsAsync()
        {
            await _cache.ClearAsync();
        }

        public async Task SetAllBrandsAsync(IEnumerable<BrandCacheEntity> brands)
        {
            if (brands == null) return;

            foreach (var brand in brands)
            {
                await _cache.AddOrUpdateAsync($"{CachePrefix}_{brand.Id}", brand);
            }

            await _cache.AddOrUpdateBulkAsync($"{CachePrefix}_all", brands);
        }

        public async Task<IEnumerable<BrandCacheEntity>> GetBrandsByIdsAsync(IEnumerable<int> ids)
        {
            var brands = new List<BrandCacheEntity>();

            foreach (var id in ids)
            {
                var brand = await _cache.GetAsync($"{CachePrefix}_{id}");
                if (brand != null)
                    brands.Add(brand);
            }

            return brands;
        }

        public async Task<bool> BrandExistsAsync(int id)
        {
            var brand = await GetBrandAsync(id);
            return brand != null;
        }
    }
}
