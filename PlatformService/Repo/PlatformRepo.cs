using AutoMapper;
using PlatformService.Interfaces;
using PlatformService.Models.Domain;
using PlatformService.Models.Entities;
using MicroServices.DataAccess.Interfaces;
using PlatformService.Classes;
using MicroServices.Caching.Interfaces;
using MicroServices.Caching.Model.Entities;

namespace PlatformService.Repo
{
    public class PlatformRepo : IPlatformDomain
    {
        private readonly IConnection DatabaseConnection;
        private readonly IPlatformDataProcessor platformDataProcessor;
        private readonly IMapper mapper;

        public PlatformRepo(
            IConnection connection,
            IPlatformDataProcessor platformDataProcessor,
            IMapper mapper)
        {
            DatabaseConnection = connection;
            this.platformDataProcessor = platformDataProcessor;
            this.mapper = mapper;
        }

        public async Task<PlatformDomainEntity> AddPlatformAsync(PlatformAddDomainEntity platform)
        {
            PlatformDomainEntity? addedPlatform = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var platformEntity = await platformDataProcessor.AddAsync(mapper.Map<PlatformAddEntity>(platform));
                addedPlatform = mapper.Map<PlatformDomainEntity>(platformEntity);
            });

            return addedPlatform!;
        }

        public async Task<PlatformDomainEntity> UpdatePlatformAsync(PlatformUpdateDomainEntity platform)
        {
            PlatformDomainEntity? updatedPlatform = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                await platformDataProcessor.UpdateAsync(mapper.Map<PlatformUpdateEntity>(platform));

                var updatedPlatformEntity = await platformDataProcessor.GetAsync(platform.Id);

                if (updatedPlatformEntity != null)
                {
                    updatedPlatform = mapper.Map<PlatformDomainEntity>(updatedPlatformEntity);
                }
            });

            if (updatedPlatform == null)
            {
                throw new KeyNotFoundException("The platform was not found or was deleted.");
            }

            return updatedPlatform;
        }

        public async Task<IEnumerable<PlatformDomainEntity>> GetAllPlatformsAsync()
        {

            IEnumerable<PlatformDomainEntity> platformDomainEntities = new List<PlatformDomainEntity>();

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var platformEntities = await platformDataProcessor.GetAllAsync();
                platformDomainEntities = platformEntities.Select(platform => mapper.Map<PlatformDomainEntity>(platform));
            });

            return platformDomainEntities!;
        }

        public async Task<IEnumerable<PlatformDomainEntity>> GetByIdsAsync(List<int> ids)
        {
            IEnumerable<PlatformDomainEntity>? platformDomainEntities = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var platformEntities = await platformDataProcessor.GetByIdsAsync(ids) ?? new List<PlatformEntity>();

                var filteredPlatforms = platformEntities
                    .Where(platform => ids.Contains(platform.Id))
                    .ToList();

                platformDomainEntities = filteredPlatforms.Select(platform => mapper.Map<PlatformDomainEntity>(platform));
            });

            return platformDomainEntities!;
        }

        public async Task<PlatformDomainEntity> GetPlatformByIdAsync(int id)
        {

            PlatformDomainEntity? platformDomainEntity = null;

            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                var platformEntity = await platformDataProcessor.GetAsync(id);

                if (platformEntity == null)
                    return;

                platformDomainEntity = mapper.Map<PlatformDomainEntity>(platformEntity);

            });

            return platformDomainEntity!;
        }

        public async Task DeletePlatformAsync(int platformId)
        {
            await DatabaseConnection.ExecuteInTransactionAsync(async () =>
            {
                await platformDataProcessor.DeletePlatformAsync(platformId);
            });
        }
    }
}