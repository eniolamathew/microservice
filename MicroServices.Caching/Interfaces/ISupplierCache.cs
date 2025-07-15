using MicroServices.Caching.Model.Entities;

namespace MicroServices.Caching.Interfaces
{
    public interface ISupplierCache
    {
        Task<SupplierCacheEntity> GetSupplierAsync(int id);
        Task<IEnumerable<SupplierCacheEntity>> GetAllSuppliersAsync();
        Task AddOrUpdateSupplierAsync(SupplierCacheEntity supplier);
        Task RemoveSupplierAsync(int id);
        Task ClearAllSuppliersAsync();
        Task SetAllSuppliersAsync(IEnumerable<SupplierCacheEntity> suppliers);
        Task<IEnumerable<SupplierCacheEntity>> GetSuppliersByIdsAsync(IEnumerable<int> ids);
        Task<bool> SupplierExistsAsync(int id);
    }
}

