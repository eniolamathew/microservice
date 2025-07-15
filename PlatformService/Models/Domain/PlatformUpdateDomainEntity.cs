namespace PlatformService.Models.Domain
{
    public class PlatformUpdateDomainEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public required string Owner { get; set; }
        public required bool IsDeleted { get; set; }
    }
}
