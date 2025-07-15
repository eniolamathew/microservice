using Microsoft.Extensions.Caching.Memory;
using MicroServices.Caching.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroServices.Caching.Model.Entities;

namespace MicroServices.Caching.Implementations
{
    public class MicroserviceCache<TCacheEntity> : IMicroserviceCache<TCacheEntity>
        where TCacheEntity : class
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan? _defaultExpiration;
        private const string AllEntitiesKey = "all_entities";

        public event Func<TCacheEntity, Task> OnEntityAdded;
        public event Func<TCacheEntity, Task> OnEntityUpdated;
        public event Func<string, Task> OnEntityRemoved;

        public MicroserviceCache(IMemoryCache memoryCache, TimeSpan? defaultExpiration = null)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _defaultExpiration = defaultExpiration;
        }

        public async Task<TCacheEntity> GetAsync(string key)
        {
            return await Task.FromResult(_memoryCache.Get<TCacheEntity>(key));
        }

        public async Task<IEnumerable<TCacheEntity>> GetAllAsync()
        {
            return await Task.FromResult(
                _memoryCache.Get<List<TCacheEntity>>(AllEntitiesKey) ?? new List<TCacheEntity>());
        }

        public async Task AddOrUpdateAsync(string key, TCacheEntity entity)
        {
            var options = new MemoryCacheEntryOptions();
            if (_defaultExpiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = _defaultExpiration;
            }

            await Task.Run(() =>
            {
                _memoryCache.Set(key, entity, options);

                // Update the all entities collection
                var allEntities = _memoryCache.Get<List<TCacheEntity>>(AllEntitiesKey) ?? new List<TCacheEntity>();
                var updatedList = new List<TCacheEntity>(allEntities);

                // Remove existing if present
                updatedList.RemoveAll(e => e.Equals(entity));
                updatedList.Add(entity);

                _memoryCache.Set(AllEntitiesKey, updatedList, options);
            });

            // Raise appropriate event
            var exists = await ExistsAsync(key);
            if (!exists && OnEntityAdded != null)
            {
                await OnEntityAdded(entity);
            }
            else if (exists && OnEntityUpdated != null)
            {
                await OnEntityUpdated(entity);
            }
        }

        public async Task RemoveAllAsync()
        {
            await Task.Run(() =>
            {
                _memoryCache.Remove(AllEntitiesKey);
            });
        }


        public async Task RemoveAsync(string key)
        {
            await Task.Run(() =>
            {
                var entity = _memoryCache.Get<TCacheEntity>(key);
                _memoryCache.Remove(key);

                var allEntities = _memoryCache.Get<List<TCacheEntity>>(AllEntitiesKey);
                if (allEntities != null && entity != null)
                {
                    var updatedList = new List<TCacheEntity>(allEntities);
                    updatedList.RemoveAll(e => e.Equals(entity));
                    _memoryCache.Set(AllEntitiesKey, updatedList);
                }
            });

            if (OnEntityRemoved != null)
            {
                await OnEntityRemoved(key);
            }
        }

        //public async Task RemoveAsync(string key)
        //{
        //    await Task.Run(() =>
        //    {
        //        var entity = _memoryCache.Get<TCacheEntity>(key);
        //        _memoryCache.Remove(key);

        //        // Update the all entities collection
        //        var allEntities = _memoryCache.Get<List<TCacheEntity>>(AllEntitiesKey);
        //        if (allEntities != null)
        //        {
        //            var updatedList = new List<TCacheEntity>(allEntities);
        //            updatedList.RemoveAll(e => e.Equals(entity));
        //            _memoryCache.Set(AllEntitiesKey, updatedList);
        //        }
        //    });

        //    if (OnEntityRemoved != null)
        //    {
        //        await OnEntityRemoved(key);
        //    }
        //}

        public async Task ClearAsync()
        {
            await Task.Run(() =>
            {
                // Clear all tracked entities
                _memoryCache.Remove(AllEntitiesKey);

                // Note: MemoryCache doesn't have a built-in Clear() method.
                // In production, you might want to track all keys separately
                // or use a different caching implementation if you need this.
            });
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }

        // Additional helper method for bulk operations

        public Task AddOrUpdateBulkAsync(string allPlatformsKey, IEnumerable<TCacheEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            if (string.IsNullOrEmpty(allPlatformsKey))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(allPlatformsKey));

            var options = new MemoryCacheEntryOptions();
            if (_defaultExpiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = _defaultExpiration;
            }

            // Get the current list of all entities or create a new one
            var currentAllEntities = _memoryCache.Get<List<TCacheEntity>>(AllEntitiesKey) ?? new List<TCacheEntity>();
            var updatedAllEntities = new List<TCacheEntity>(currentAllEntities);

            foreach (var entity in entities)
            {
                if (entity == null) continue;

                // Generate cache key using entity's hash code
                var cacheKey = $"entity_{entity.GetHashCode()}";

                _memoryCache.Set(cacheKey, entity, options);

                // Update the master list
                updatedAllEntities.RemoveAll(e => e.Equals(entity));
                updatedAllEntities.Add(entity);
            }

            // Update both the specific allPlatformsKey collection and the global AllEntitiesKey
            _memoryCache.Set(allPlatformsKey, entities.Where(e => e != null).ToList(), options);
            _memoryCache.Set(AllEntitiesKey, updatedAllEntities, options);

            return Task.CompletedTask;
        }
    }
}



//public async Task AddOrUpdateBulkAsync(IDictionary<string, TCacheEntity> entities)
//{
//    var options = new MemoryCacheEntryOptions();
//    if (_defaultExpiration.HasValue)
//    {
//        options.AbsoluteExpirationRelativeToNow = _defaultExpiration;
//    }

//    await Task.Run(() =>
//    {
//        foreach (var (key, entity) in entities)
//        {
//            _memoryCache.Set(key, entity, options);
//        }

//        // Update all entities collection
//        var allEntities = _memoryCache.Get<List<TCacheEntity>>(AllEntitiesKey) ?? new List<TCacheEntity>();
//        var updatedList = new List<TCacheEntity>(allEntities);

//        foreach (var entity in entities.Values)
//        {
//            updatedList.RemoveAll(e => e.Equals(entity));
//            updatedList.Add(entity);
//        }

//        _memoryCache.Set(AllEntitiesKey, updatedList, options);
//    });
//}