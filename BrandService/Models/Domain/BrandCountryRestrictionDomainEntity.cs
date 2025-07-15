using System.Diagnostics.Metrics;

namespace BrandService.Models.Domain
{
    public class BrandCountryRestrictionDomainEntity
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public int CountryId { get; set; }
        public BrandDomainEntity Brand { get; set; }
    }
}
