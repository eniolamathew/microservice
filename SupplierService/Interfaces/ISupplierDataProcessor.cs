using SupplierService.Models.Domain;
using SupplierService.Models.Entities;
using MicroServices.DataAccess.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupplierService.Interfaces
{
    public interface ISupplierDataProcessor
    {
        Task<SupplierEntity?> GetAsync(int id);

        Task<SupplierEntity?> AddAsync(SupplierAddEntity supplier);

        Task UpdateAsync(SupplierUpdateEntity supplier);

        Task<SupplierEntity> DeleteSupplierAsync(int supplierId);

        Task<IEnumerable<SupplierEntity>> GetAllAsync();

        Task<IEnumerable<SupplierEntity>> GetByIdsAsync(List<int> ids);
    }
}
