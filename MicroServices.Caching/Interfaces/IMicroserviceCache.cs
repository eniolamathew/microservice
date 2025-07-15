using MicroServices.Caching.Model.Entities;

namespace MicroServices.Caching.Interfaces
{
    public interface IMicroserviceCache<TCacheEntity> where TCacheEntity : class
    {
        Task<TCacheEntity> GetAsync(string key);
        Task<IEnumerable<TCacheEntity>> GetAllAsync();
        Task AddOrUpdateAsync(string key, TCacheEntity entity);
        Task RemoveAsync(string key);
        Task RemoveAllAsync();
        Task ClearAsync();
        Task<bool> ExistsAsync(string key);
        Task AddOrUpdateBulkAsync(string allPlatformsKey, IEnumerable<TCacheEntity> platformsFromApi);

        event Func<TCacheEntity, Task> OnEntityAdded;
        event Func<TCacheEntity, Task> OnEntityUpdated;
        event Func<string, Task> OnEntityRemoved;
    }
}