using PlatformService.Models;
using PlatformService.Models.Domain;
using PlatformService.Models.Entities;

namespace PlatformService.Interfaces
{
    public interface IPlatformDomain
    {
        Task<PlatformDomainEntity> AddPlatformAsync(PlatformAddDomainEntity platform);

        Task<PlatformDomainEntity> UpdatePlatformAsync(PlatformUpdateDomainEntity platform);

        Task<IEnumerable<PlatformDomainEntity>> GetAllPlatformsAsync();

        Task <PlatformDomainEntity> GetPlatformByIdAsync(int id);

        Task<IEnumerable<PlatformDomainEntity>> GetByIdsAsync(List<int> ids);

        Task DeletePlatformAsync(int platformId);
    }
}
