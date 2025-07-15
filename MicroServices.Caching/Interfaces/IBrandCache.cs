using MicroServices.Caching.Model.Entities;

namespace MicroServices.Caching.Interfaces
{
    public interface IBrandCache
    {
        Task<BrandCacheEntity> GetBrandAsync(int id);
        Task<IEnumerable<BrandCacheEntity>> GetAllBrandsAsync();
        Task AddOrUpdateBrandAsync(BrandCacheEntity brand);
        Task RemoveBrandAsync(int id);
        Task ClearAllBrandsAsync();
        Task SetAllBrandsAsync(IEnumerable<BrandCacheEntity> brands);
        Task<IEnumerable<BrandCacheEntity>> GetBrandsByIdsAsync(IEnumerable<int> ids);
        Task<bool> BrandExistsAsync(int id);
    }
}
