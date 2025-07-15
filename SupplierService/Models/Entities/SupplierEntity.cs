using LinqToDB.Mapping;

namespace SupplierService.Models.Entities
{
    [Table(Schema = "supplier", Name = "suppliers")]
    public class SupplierEntity
    {
        [PrimaryKey, Identity, Column(Name = "id"), NotNull]
        public int Id { get; set; }

        [Column(Name = "supplierName"), NotNull]
        public string SupplierName { get; set; } = string.Empty;

        [Column(Name = "description"), NotNull]
        public string Description { get; set; } = string.Empty;

        [Column(Name = "isdeleted"), NotNull]
        public bool IsDeleted { get; set; }
    }
}


