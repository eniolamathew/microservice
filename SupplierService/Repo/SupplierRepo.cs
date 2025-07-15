using AutoMapper;
using SupplierService.Interfaces;
using SupplierService.Models.Domain;
using SupplierService.Models.Entities;
using MicroServices.DataAccess.Interfaces;
using SupplierService.Classes;
using MicroServices.Caching.Interfaces;
using MicroServices.Caching.Model.Entities;

namespace SupplierService.Repo
{
    public class SupplierRepo : ISupplierDomain
    {
        private readonly IConnection DatabaseConnection;
        private readonly ISupplierDataProcessor supplierDataProcessor;
        private readonly IMapper mapper;

        public SupplierRepo(
            IConnection connection,
            ISupplierDataProcessor supplierDataProcessor,
            IMapper mapper)
        {
            DatabaseConnection = connection;
            this.supplierDataProcessor = supplierDataProcessor;
            this.mapper = mapper;
        }

        public async Task<SupplierDomainEntity> AddSupplierAsync(SupplierAddDomainEntity supplier)
        {
            SupplierDomainEntity? addedSupplier = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var supplierEntity = await supplierDataProcessor.AddAsync(mapper.Map<SupplierAddEntity>(supplier));
                addedSupplier = mapper.Map<SupplierDomainEntity>(supplierEntity);
            });

            return addedSupplier!;
        }

        public async Task<SupplierDomainEntity> UpdateSupplierAsync(SupplierUpdateDomainEntity supplier)
        {
            SupplierDomainEntity? updatedSupplier = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                await supplierDataProcessor.UpdateAsync(mapper.Map<SupplierUpdateEntity>(supplier));

                var updatedSupplierEntity = await supplierDataProcessor.GetAsync(supplier.Id);

                if (updatedSupplierEntity != null)
                {
                    updatedSupplier = mapper.Map<SupplierDomainEntity>(updatedSupplierEntity);
                }
            });

            if (updatedSupplier == null)
            {
                throw new KeyNotFoundException("The supplier was not found or was deleted.");
            }

            return updatedSupplier;
        }

        public async Task<IEnumerable<SupplierDomainEntity>> GetAllSuppliersAsync()
        {
            IEnumerable<SupplierDomainEntity> supplierDomainEntities = new List<SupplierDomainEntity>();

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var supplierEntities = await supplierDataProcessor.GetAllAsync();
                supplierDomainEntities = supplierEntities.Select(supplier => mapper.Map<SupplierDomainEntity>(supplier));
            });

            return supplierDomainEntities!;
        }

        public async Task<IEnumerable<SupplierDomainEntity>> GetByIdsAsync(List<int> ids)
        {
            IEnumerable<SupplierDomainEntity>? supplierDomainEntities = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var supplierEntities = await supplierDataProcessor.GetByIdsAsync(ids) ?? new List<SupplierEntity>();

                var filteredSuppliers = supplierEntities
                    .Where(supplier => ids.Contains(supplier.Id))
                    .ToList();

                supplierDomainEntities = filteredSuppliers.Select(supplier => mapper.Map<SupplierDomainEntity>(supplier));
            });

            return supplierDomainEntities!;
        }

        public async Task<SupplierDomainEntity> GetSupplierByIdAsync(int id)
        {
            SupplierDomainEntity? supplierDomainEntity = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var supplierEntity = await supplierDataProcessor.GetAsync(id);

                if (supplierEntity == null)
                    return;

                supplierDomainEntity = mapper.Map<SupplierDomainEntity>(supplierEntity);
            });

            return supplierDomainEntity!;
        }

        public async Task DeleteSupplierAsync(int supplierId)
        {
            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                await supplierDataProcessor.DeleteSupplierAsync(supplierId);
            });
        }
    }
}

