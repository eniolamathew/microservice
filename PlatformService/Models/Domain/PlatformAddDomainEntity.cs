namespace PlatformService.Models.Domain
{
    public class PlatformAddDomainEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public required string Owner { get; set; }
        public required bool IsDeleted { get; set; }
    }
}