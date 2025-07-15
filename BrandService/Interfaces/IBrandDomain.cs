using BrandService.Models.Domain;

namespace BrandService.Interfaces
{
    public interface IBrandDomain
    {
        Task<BrandDomainEntity> AddBrandAsync(BrandAddDomainEntity brand);

        Task<BrandDomainEntity> UpdateBrandAsync(BrandUpdateDomainEntity brand);

        Task<IEnumerable<BrandDomainEntity>> GetAllBrandsAsync();

        Task<BrandDomainEntity> GetBrandByIdAsync(int id);

        Task<IEnumerable<BrandDomainEntity>> GetByIdsAsync(List<int> ids);

        Task DeleteBrandAsync(int platformId);
    }
}
