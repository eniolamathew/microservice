using SupplierService.Models.Domain;

namespace SupplierService.Interfaces
{
    public interface ISupplierDomain
    {
        Task<SupplierDomainEntity> AddSupplierAsync(SupplierAddDomainEntity supplier);

        Task<SupplierDomainEntity> UpdateSupplierAsync(SupplierUpdateDomainEntity supplier);

        Task<IEnumerable<SupplierDomainEntity>> GetAllSuppliersAsync();

        Task<SupplierDomainEntity> GetSupplierByIdAsync(int id);

        Task<IEnumerable<SupplierDomainEntity>> GetByIdsAsync(List<int> ids);

        Task DeleteSupplierAsync(int supplierId);
    }
}
