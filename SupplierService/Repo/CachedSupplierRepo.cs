using AutoMapper;
using SupplierService.Interfaces;
using SupplierService.Models.Domain;
using MicroServices.Caching.Interfaces;
using MicroServices.Caching.Model.Entities;
using MicroServices.Caching.ServiceCaches;

namespace SupplierService.Repo.Decorators
{
    public class CachedSupplierRepo : ISupplierDomain
    {
        private readonly ISupplierDomain _innerRepo;
        private readonly ISupplierCache _supplierCache;
        private readonly IMapper _mapper;

        public CachedSupplierRepo(ISupplierDomain innerRepo, ISupplierCache supplierCache, IMapper mapper)
        {
            _innerRepo = innerRepo;
            _supplierCache = supplierCache;
            _mapper = mapper;
        }

        public async Task<SupplierDomainEntity> AddSupplierAsync(SupplierAddDomainEntity supplier)
        {
            var result = await _innerRepo.AddSupplierAsync(supplier);
            await _supplierCache.AddOrUpdateSupplierAsync(_mapper.Map<SupplierCacheEntity>(result));
            return result;
        }

        public async Task<SupplierDomainEntity> UpdateSupplierAsync(SupplierUpdateDomainEntity supplier)
        {
            var result = await _innerRepo.UpdateSupplierAsync(supplier);
            await _supplierCache.AddOrUpdateSupplierAsync(_mapper.Map<SupplierCacheEntity>(result));
            return result;
        }

        public async Task<IEnumerable<SupplierDomainEntity>> GetAllSuppliersAsync()
        {
            var cached = await _supplierCache.GetAllSuppliersAsync();
            if (cached != null && cached.Any())
            {
                return cached.Select(s => _mapper.Map<SupplierDomainEntity>(s));
            }

            // Refresh cache
            var supplierEntities = await _innerRepo.GetAllSuppliersAsync();
            var cacheEntities = supplierEntities.Select(s => _mapper.Map<SupplierCacheEntity>(s)).ToList();

            await _supplierCache.SetAllSuppliersAsync(cacheEntities);
            return supplierEntities;
        }

        public async Task<IEnumerable<SupplierDomainEntity>> GetByIdsAsync(List<int> ids)
        {
            var cachedEntities = await _supplierCache.GetSuppliersByIdsAsync(ids);
            var foundIds = cachedEntities.Select(s => s.Id).ToHashSet();

            var missingIds = ids.Where(id => !foundIds.Contains(id)).ToList();
            var results = new List<SupplierDomainEntity>(
                cachedEntities.Select(s => _mapper.Map<SupplierDomainEntity>(s))
            );

            if (missingIds.Any())
            {
                var freshEntities = await _innerRepo.GetByIdsAsync(missingIds);

                // Cache the fresh results
                var cacheEntities = freshEntities.Select(s => _mapper.Map<SupplierCacheEntity>(s));
                foreach (var entity in cacheEntities)
                {
                    await _supplierCache.AddOrUpdateSupplierAsync(entity);
                }

                results.AddRange(freshEntities);
            }

            return results;
        }

        public async Task<SupplierDomainEntity> GetSupplierByIdAsync(int id)
        {
            var cached = await _supplierCache.GetSupplierAsync(id);
            if (cached != null)
            {
                return _mapper.Map<SupplierDomainEntity>(cached);
            }

            var result = await _innerRepo.GetSupplierByIdAsync(id);
            if (result != null)
            {
                await _supplierCache.AddOrUpdateSupplierAsync(_mapper.Map<SupplierCacheEntity>(result));
            }
            return result;
        }

        public async Task DeleteSupplierAsync(int supplierId)
        {
            await _innerRepo.DeleteSupplierAsync(supplierId);
            await _supplierCache.RemoveSupplierAsync(supplierId);
        }
    }
}
