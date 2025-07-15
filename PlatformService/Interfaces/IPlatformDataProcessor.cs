using PlatformService.Models.Domain;
using PlatformService.Models.Entities;
using MicroServices.DataAccess.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlatformService.Interfaces
{
    public interface IPlatformDataProcessor
    {
        Task<PlatformEntity?> GetAsync(int id);

        Task<PlatformEntity?> AddAsync(PlatformAddEntity platform);

        Task UpdateAsync(PlatformUpdateEntity platform);

        Task<PlatformEntity> DeletePlatformAsync(int platformId);

        Task<IEnumerable<PlatformEntity>> GetAllAsync();

        Task<IEnumerable<PlatformEntity>> GetByIdsAsync(List<int> ids);
    }
}
