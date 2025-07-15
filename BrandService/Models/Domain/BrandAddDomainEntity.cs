using MicroServices.API.Enums;

namespace BrandService.Models.Domain
{
    public class BrandAddDomainEntity
    {
        public string Description { get; set; }

        public int SupplierId { get; set; }

        public BrandStatuses StatusId { get; set; }

        public bool ShowInNavigation { get; set; }
    }
}


