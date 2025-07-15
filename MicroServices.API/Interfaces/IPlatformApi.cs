using MicroServices.API.Common;
using MicroServices.API.Models.Entities;

namespace MicroServices.API.Interfaces
{
    public interface IPlatformApi
    {
        Task<ApiResult<IEnumerable<PlatformDto>>> GetAllAsync();
        Task<ApiResult<PlatformDto>> GetByIdAsync(int id);
        Task<ApiResult<IEnumerable<PlatformDto>>> GetByIdsAsync(IEnumerable<int> ids);
    }
}
