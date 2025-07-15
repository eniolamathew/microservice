using BrandService.Enums;
using LinqToDB.Mapping;
using MicroServices.API.Enums;

namespace BrandService.Models.Entities
{
    [Table(Schema = "brand", Name = "brands")]
    public class BrandAddEntity
    {
        [PrimaryKey, Identity, Column(Name = "id"), NotNull]
        public int Id { get; set; }

        [Column(Name = "description"), NotNull]
        public string Description { get; set; } = string.Empty;

        [Column(Name = "supplierId"), NotNull]
        public int SupplierId { get; set; }

        [Column(Name = "statusId"), NotNull]
        public BrandStatuses StatusId { get; set; }

        [Column(Name = "showInNavigation"), NotNull]
        public bool ShowInNavigation { get; set; }

    }
}
