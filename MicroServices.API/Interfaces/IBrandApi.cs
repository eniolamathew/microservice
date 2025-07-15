using MicroServices.API.Common;
using MicroServices.API.Models.Entities;

namespace MicroServices.API.Interfaces
{
    public interface IBrandApi
    {
        Task<ApiResult<IEnumerable<BrandDto>>> GetAllAsync();
        Task<ApiResult<BrandDto>> GetByIdAsync(int id);
        Task<ApiResult<IEnumerable<BrandDto>>> GetByIdsAsync(IEnumerable<int> ids);
    }
}