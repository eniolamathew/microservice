using LinqToDB.Mapping;
using MicroServices.API.Enums;

namespace BrandService.Models.Entities
{
    [Table(Schema = "brand", Name = "brands")]
    public class BrandUpdateEntity
    {
        [PrimaryKey, Identity, Column(Name = "id"), NotNull]
        public int Id { get; set; }

        [Column(Name = "name"), NotNull]
        public string Name { get; set; } = string.Empty;

        [Column(Name = "description"), NotNull]
        public string Description { get; set; } = string.Empty;

        [Column(Name = "supplierId"), NotNull]
        public int SupplierId { get; set; }

        [Column(Name = "supplierName"), NotNull]
        public string SupplierName { get; set; } = string.Empty;

        [Column(Name = "importId"), NotNull]
        public string ImportId { get; set; } = string.Empty;

        [Column(Name = "isdeleted"), NotNull]
        public bool IsDeleted { get; set; }

        [Column(Name = "statusId"), NotNull]
        public BrandStatuses StatusId { get; set; }

        [Column(Name = "showInNavigation"), NotNull]
        public bool ShowInNavigation { get; set; }

        [Column(Name = "CountryRestrictionTypeId"), NotNull]
        public int CountryRestrictionTypeId { get; set; }

        [Column(Name = "articleContentFilterId"), NotNull]
        public BrandArticleContentFilters ArticleContentFilterId { get; set; }

        [Association(ThisKey = nameof(Id), OtherKey = nameof(BrandCountryRestrictionEntity.BrandId))]
        public List<int> CountryRestrictions { get; set; } = new();
    }
}
