using BrandService.Models.Domain;
using LinqToDB.Mapping;

namespace BrandService.Models.Entities
{
    [Table(Schema = "brand", Name = "brand_country_restriction")]
    public class BrandCountryRestrictionEntity
    {
        [Column("id"), PrimaryKey, Identity]
        public int Id { get; set; }

        [Column("brand_id")]
        public int BrandId { get; set; }

        [Column("country_id")]
        public int CountryId { get; set; }
    }
}
