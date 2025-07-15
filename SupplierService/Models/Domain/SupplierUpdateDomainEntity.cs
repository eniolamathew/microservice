namespace SupplierService.Models.Domain
{
    public class SupplierUpdateDomainEntity
    {
        public int Id { get; set; }
        public required string SupplierName { get; set; }
        public required string Description { get; set; }
        public required bool IsDeleted { get; set; }
    }
}
