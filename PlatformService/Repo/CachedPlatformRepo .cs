using AutoMapper;
using PlatformService.Interfaces;
using PlatformService.Models.Domain;
using MicroServices.Caching.Interfaces;
using MicroServices.Caching.Model.Entities;

namespace PlatformService.Repo.Decorators
{
    public class CachedPlatformRepo : IPlatformDomain
    {
        private readonly IPlatformDomain _innerRepo;
        private readonly IPlatformCache _platformCache;
        private readonly IMapper _mapper;

        public CachedPlatformRepo( IPlatformDomain innerRepo, IPlatformCache platformCache,  IMapper mapper)
        {
            _innerRepo = innerRepo;
            _platformCache = platformCache;
            _mapper = mapper;
        }

        public async Task<PlatformDomainEntity> AddPlatformAsync(PlatformAddDomainEntity platform)
        {
            var result = await _innerRepo.AddPlatformAsync(platform);
            await _platformCache.AddOrUpdatePlatformAsync(_mapper.Map<PlatformCacheEntity>(result));
            return result;
        }


        public async Task<PlatformDomainEntity> UpdatePlatformAsync(PlatformUpdateDomainEntity platform)
        {
            var result = await _innerRepo.UpdatePlatformAsync(platform);
            await _platformCache.AddOrUpdatePlatformAsync(_mapper.Map<PlatformCacheEntity>(result));
            return result;
        }

        public async Task<IEnumerable<PlatformDomainEntity>> GetAllPlatformsAsync()
        {
            var cached = await _platformCache.GetAllPlatformsAsync();
            if (cached != null && cached.Any())
            {
                return cached.Select(p => _mapper.Map<PlatformDomainEntity>(p));
            }

            // Refresh cache
            var platformEntities = await _innerRepo.GetAllPlatformsAsync();
            var cacheEntities = platformEntities.Select(p => _mapper.Map<PlatformCacheEntity>(p)).ToList();
            
            
            await _platformCache.SetAllPlatformsAsync(cacheEntities);
            return platformEntities;
        }

        public async Task<IEnumerable<PlatformDomainEntity>> GetByIdsAsync(List<int> ids)
        {
            var cachedEntities = await _platformCache.GetPlatformsByIdsAsync(ids);
            var foundIds = cachedEntities.Select(p => p.Id).ToHashSet();

            var missingIds = ids.Where(id => !foundIds.Contains(id)).ToList();
            var results = new List<PlatformDomainEntity>(
                cachedEntities.Select(p => _mapper.Map<PlatformDomainEntity>(p))
            );

            if (missingIds.Any())
            {
                var freshEntities = await _innerRepo.GetByIdsAsync(missingIds);

                // Cache the fresh results
                var cacheEntities = freshEntities.Select(p => _mapper.Map<PlatformCacheEntity>(p));
                foreach (var entity in cacheEntities)
                {
                    await _platformCache.AddOrUpdatePlatformAsync(entity);
                }

                results.AddRange(freshEntities);
            }

            return results;
        }

        public async Task<PlatformDomainEntity> GetPlatformByIdAsync(int id)
        {
            var cached = await _platformCache.GetPlatformAsync(id);
            if (cached != null)
            {
                return _mapper.Map<PlatformDomainEntity>(cached);
            }

            var result = await _innerRepo.GetPlatformByIdAsync(id);
            if (result != null)
            {
                await _platformCache.AddOrUpdatePlatformAsync(_mapper.Map<PlatformCacheEntity>(result));
            }
            return result;
        }

        public async Task DeletePlatformAsync(int platformId)
        {
            await _innerRepo.DeletePlatformAsync(platformId);
            await _platformCache.RemovePlatformAsync(platformId);
        }
    }
}