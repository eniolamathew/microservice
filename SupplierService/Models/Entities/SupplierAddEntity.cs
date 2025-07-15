using LinqToDB.Mapping;

namespace SupplierService.Models.Entities
{
    [Table(Schema = "supplier", Name = "suppliers")]
    public class SupplierAddEntity
    {
        [PrimaryKey, Identity, Column(Name = "id"), NotNull]
        public int Id { get; set; }

        [Column(Name = "supplierName"), NotNull]
        public string SupplierNameName { get; set; } = string.Empty;

        [Column(Name = "description"), NotNull]
        public string Description { get; set; } = string.Empty;

        [Column(Name = "isdeleted"), NotNull]
        public bool IsDeleted { get; set; }
    }
}



