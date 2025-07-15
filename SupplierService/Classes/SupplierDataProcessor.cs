using LinqToDB;
using SupplierService.Interfaces;
using SupplierService.Models.Entities;
using MicroServices.DataAccess.Interfaces;

namespace SupplierService.Classes
{
    public class SupplierDataProcessor : ISupplierDataProcessor
    {
        private readonly IConnection DataBaseConnection;

        public ITable<SupplierEntity> Suppliers => DataBaseConnection.GetTable<SupplierEntity>();

        public IConnection Connection => DataBaseConnection;

        public SupplierDataProcessor(IConnection connection)
        {
            DataBaseConnection = connection;
        }

        public async Task<SupplierEntity?> GetAsync(int id)
        {
            return await Suppliers.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<SupplierEntity>> GetAllAsync()
        {
            return await Suppliers.Where(a => !a.IsDeleted).ToListAsync();
        }

        public async Task<SupplierEntity?> AddAsync(SupplierAddEntity supplier)
        {
            var id = await DataBaseConnection.InsertWithInt32IdentityAsync(supplier);
            return await GetAsync(id);
        }

        public async Task UpdateAsync(SupplierUpdateEntity supplier)
        {
            if (supplier.Id <= 0)
            {
                throw new ArgumentException("Invalid supplier Id.");
            }

            var existingSupplier = await Suppliers.FirstOrDefaultAsync(s => s.Id == supplier.Id);

            if (existingSupplier != null)
            {
                existingSupplier.SupplierName = supplier.SupplierName;
                existingSupplier.Description = supplier.Description;
                existingSupplier.IsDeleted = supplier.IsDeleted;

                await DataBaseConnection.UpdateAsync(existingSupplier);
            }
            else
            {
                throw new KeyNotFoundException("Supplier not found.");
            }
        }

        public async Task<IEnumerable<SupplierEntity>> GetByIdsAsync(List<int> ids)
        {
            return await Suppliers.Where(s => ids.Contains(s.Id)).ToListAsync();
        }

        public async Task<SupplierEntity> DeleteSupplierAsync(int supplierId)
        {
            await Suppliers.Where(a => a.Id == supplierId).Set(a => a.IsDeleted, true).UpdateAsync();
            return Suppliers.Single(a => a.Id == supplierId);
        }
    }
}
