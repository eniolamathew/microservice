namespace MicroServices.API.Models.Entities
{
    public class PlatformDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public required string Owner { get; set; }
        public required bool IsDeleted { get; set; }
    }
}
