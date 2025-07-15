using LinqToDB.Mapping;

namespace PlatformService.Models.Entities
{
    [Table(Schema = "platform", Name = "platforms")]
    public class PlatformEntity
    {
        [PrimaryKey, Identity, Column(Name = "id"), NotNull]
        public int Id { get; set; }

        [Column(Name = "name"), NotNull]
        public string Name { get; set; } = string.Empty;

        [Column(Name = "description"), NotNull]
        public string Description { get; set; } = string.Empty;

        [Column(Name = "price"), NotNull]
        public decimal Price { get; set; }

        [Column(Name = "owner"), NotNull]
        public string Owner { get; set; } = string.Empty;

        [Column(Name = "isdeleted"), NotNull]
        public bool IsDeleted { get; set; }
    }
}

