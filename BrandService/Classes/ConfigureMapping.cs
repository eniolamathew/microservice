using AutoMapper;
using MicroServices.Caching.Model.Entities;
using BrandService.Models.Domain;
using BrandService.Models.Entities;

namespace BrandService.Classes
{
    public class ConfigureMapping : Profile
    {
        public ConfigureMapping()
        {
            // Domain to Entity
            CreateMap<BrandAddEntity, BrandEntity>();
            CreateMap<BrandAddDomainEntity, BrandAddEntity>();
            CreateMap<BrandUpdateDomainEntity, BrandUpdateEntity>();
            CreateMap<BrandEntity, BrandDomainEntity>();

            // Entity to Domain
            CreateMap<BrandAddEntity, BrandAddDomainEntity>();
            CreateMap<BrandUpdateEntity, BrandUpdateDomainEntity>();
            CreateMap<BrandDomainEntity, BrandEntity>();

            // Domain to Cache
            CreateMap<BrandDomainEntity, BrandCacheEntity>()
                .ReverseMap();

        }
    }
}


