namespace MicroServices.Caching.Model.Entities;
public class PlatformCacheEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required string Owner { get; set; }
    public required bool IsDeleted { get; set; }

}