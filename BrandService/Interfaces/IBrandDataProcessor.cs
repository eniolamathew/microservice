using BrandService.Models.Entities;

namespace BrandService.Interfaces
{
    public interface IBrandDataProcessor
    {
        Task<BrandEntity?> GetAsync(int id);

        Task<BrandEntity?> AddAsync(BrandAddEntity brand);

        Task UpdateAsync(BrandUpdateEntity brand);

        Task<BrandEntity> DeleteBrandAsync(int brandId);

        Task<IEnumerable<BrandEntity>> GetAllAsync();

        Task<IEnumerable<BrandEntity>> GetByIdsAsync(List<int> ids);
    }
}