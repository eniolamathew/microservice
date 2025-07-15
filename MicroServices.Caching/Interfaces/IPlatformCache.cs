using System.Collections.Generic;
using System.Threading.Tasks;
using MicroServices.Caching.Model.Entities;

namespace MicroServices.Caching.Interfaces
{
    public interface IPlatformCache
    {
        Task<PlatformCacheEntity> GetPlatformAsync(int id);
        Task<IEnumerable<PlatformCacheEntity>> GetAllPlatformsAsync();
        Task AddOrUpdatePlatformAsync(PlatformCacheEntity platform);
        Task RemovePlatformAsync(int id);
        Task ClearAllPlatformsAsync();
        Task SetAllPlatformsAsync(IEnumerable<PlatformCacheEntity> platforms);
        Task<IEnumerable<PlatformCacheEntity>> GetPlatformsByIdsAsync(IEnumerable<int> ids);
        Task<bool> PlatformExistsAsync(int id);
    }
}