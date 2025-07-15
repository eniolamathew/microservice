using AutoMapper;
using BrandService.Models.Domain;
using BrandService.Models.Entities;
using MicroServices.DataAccess.Interfaces;
using BrandService.Classes;
using BrandService.Interfaces;

namespace BrandService.Repo.Decorators
{
    public class BrandRepo : IBrandDomain
    {
        private readonly IConnection DatabaseConnection;
        private readonly IBrandDataProcessor brandDataProcessor;
        private readonly IMapper mapper;

        public BrandRepo(
            IConnection connection,
            IBrandDataProcessor brandDataProcessor,
            IMapper mapper)
        {
            DatabaseConnection = connection;
            this.brandDataProcessor = brandDataProcessor;
            this.mapper = mapper;
        }

        public async Task<BrandDomainEntity> AddBrandAsync(BrandAddDomainEntity brand)
        {
            BrandDomainEntity? addedBrand = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var brandEntity = await brandDataProcessor.AddAsync(mapper.Map<BrandAddEntity>(brand));
                addedBrand = mapper.Map<BrandDomainEntity>(brandEntity);
            });

            return addedBrand!;
        }
       
        public async Task<BrandDomainEntity> UpdateBrandAsync(BrandUpdateDomainEntity platform)
        {
            BrandDomainEntity? updatedPlatform = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                await brandDataProcessor.UpdateAsync(mapper.Map<BrandUpdateEntity>(platform));

                var updatedPlatformEntity = await brandDataProcessor.GetAsync(platform.Id);

                if (updatedPlatformEntity != null)
                {
                    updatedPlatform = mapper.Map<BrandDomainEntity>(updatedPlatformEntity);
                }
            });

            if (updatedPlatform == null)
            {
                throw new KeyNotFoundException("The platform was not found or was deleted.");
            }

            return updatedPlatform;
        }

        public async Task<IEnumerable<BrandDomainEntity>> GetAllBrandsAsync()
        {

            IEnumerable<BrandDomainEntity> platformDomainEntities = new List<BrandDomainEntity>();

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var platformEntities = await brandDataProcessor.GetAllAsync();
                platformDomainEntities = platformEntities.Select(platform => mapper.Map<BrandDomainEntity>(platform));
            });

            return platformDomainEntities!;
        }

        public async Task<IEnumerable<BrandDomainEntity>> GetByIdsAsync(List<int> ids)
        {
            IEnumerable<BrandDomainEntity>? platformDomainEntities = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var platformEntities = await brandDataProcessor.GetByIdsAsync(ids) ?? new List<BrandEntity>();

                var filteredPlatforms = platformEntities
                    .Where(platform => ids.Contains(platform.Id))
                    .ToList();

                platformDomainEntities = filteredPlatforms.Select(platform => mapper.Map<BrandDomainEntity>(platform));
            });

            return platformDomainEntities!;
        }

        public async Task<BrandDomainEntity> GetBrandByIdAsync(int id)
        {

            BrandDomainEntity? platformDomainEntity = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var platformEntity = await brandDataProcessor.GetAsync(id);

                if (platformEntity == null)
                    return;

                platformDomainEntity = mapper.Map<BrandDomainEntity>(platformEntity);

            });

            return platformDomainEntity!;
        }

        public async Task DeleteBrandAsync(int platformId)
        {
            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                await brandDataProcessor.DeleteBrandAsync(platformId);
            });
        }
    }
}
