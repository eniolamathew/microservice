using AutoMapper;
using BrandService.Interfaces;
using BrandService.Models.Domain;
using MicroServices.Caching.Interfaces;
using MicroServices.Caching.Model.Entities;

namespace BrandService.Repo
{
    public class CachedBrandRepo : IBrandDomain
    {
        private readonly IBrandDomain _innerRepo;
        private readonly IBrandCache _brandCache;
        private readonly IMapper _mapper;

        public CachedBrandRepo(IBrandDomain innerRepo, IBrandCache brandCache, IMapper mapper)
        {
            _innerRepo = innerRepo;
            _brandCache = brandCache;
            _mapper = mapper;
        }

        public async Task<BrandDomainEntity> AddBrandAsync(BrandAddDomainEntity brand)
        {
            var result = await _innerRepo.AddBrandAsync(brand);
            await _brandCache.AddOrUpdateBrandAsync(_mapper.Map<BrandCacheEntity>(result));
            return result;
        }


        public async Task<BrandDomainEntity> UpdateBrandAsync(BrandUpdateDomainEntity brand)
        {
            var result = await _innerRepo.UpdateBrandAsync(brand);
            await _brandCache.AddOrUpdateBrandAsync(_mapper.Map<BrandCacheEntity>(result));
            return result;
        }

        public async Task<IEnumerable<BrandDomainEntity>> GetAllBrandsAsync()
        {
            var cached = await _brandCache.GetAllBrandsAsync();
            if (cached != null && cached.Any())
            {
                return cached.Select(p => _mapper.Map<BrandDomainEntity>(p));
            }

            // Refresh cache
            var brandEntities = await _innerRepo.GetAllBrandsAsync();
            var cacheEntities = brandEntities.Select(p => _mapper.Map<BrandCacheEntity>(p)).ToList();
            await _brandCache.SetAllBrandsAsync(cacheEntities);
            return brandEntities;
        }

        public async Task<IEnumerable<BrandDomainEntity>> GetByIdsAsync(List<int> ids)
        {
            var cachedEntities = await _brandCache.GetBrandsByIdsAsync(ids);
            var foundIds = cachedEntities.Select(p => p.Id).ToHashSet();

            var missingIds = ids.Where(id => !foundIds.Contains(id)).ToList();
            var results = new List<BrandDomainEntity>(
                cachedEntities.Select(p => _mapper.Map<BrandDomainEntity>(p))
            );

            if (missingIds.Any())
            {
                var freshEntities = await _innerRepo.GetByIdsAsync(missingIds);

                // Cache the fresh results
                var cacheEntities = freshEntities.Select(p => _mapper.Map<BrandCacheEntity>(p));
                foreach (var entity in cacheEntities)
                {
                    await _brandCache.AddOrUpdateBrandAsync(entity);
                }

                results.AddRange(freshEntities);
            }

            return results;
        }

        public async Task<BrandDomainEntity> GetBrandByIdAsync(int id)
        {
            var cached = await _brandCache.GetBrandAsync(id);
            if (cached != null)
            {
                return _mapper.Map<BrandDomainEntity>(cached);
            }

            var result = await _innerRepo.GetBrandByIdAsync(id);
            if (result != null)
            {
                await _brandCache.AddOrUpdateBrandAsync(_mapper.Map<BrandCacheEntity>(result));
            }
            return result;
        }

        public async Task DeleteBrandAsync(int platformId)
        {
            await _innerRepo.DeleteBrandAsync(platformId);
            await _brandCache.RemoveBrandAsync(platformId);
        }
    }
}