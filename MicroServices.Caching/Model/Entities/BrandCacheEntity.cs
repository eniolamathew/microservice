using MicroServices.API.Enums;

namespace MicroServices.Caching.Model.Entities
{
    public class BrandCacheEntity
    {
        public int Id { get; set; }

        public required string Description { get; set; }

        public int SupplierId { get; set; }

        public required string SupplierName { get; set; }

        public string ImportId { get; set; }

        public bool IsDeleted { get; set; }

        public BrandStatuses StatusId { get; set; }

        public bool ShowInNavigation { get; set; }

        public required int[] CountryRestrictions { get; set; }

        public int CountryRestrictionTypeId { get; set; }

        public BrandArticleContentFilters ArticleContentFilterId { get; set; }

    }
}

