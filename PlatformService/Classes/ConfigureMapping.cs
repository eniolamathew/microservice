using AutoMapper;
using MicroServices.Caching.Model.Entities;
using PlatformService.Models.Domain;
using PlatformService.Models.Entities;

namespace PlatformService.Classes
{
    public class ConfigureMapping : Profile
    {
        public ConfigureMapping()
        {
            // Domain to Entity
            CreateMap<PlatformAddEntity, PlatformEntity>();
            CreateMap<PlatformAddDomainEntity, PlatformAddEntity>();
            CreateMap<PlatformUpdateDomainEntity, PlatformUpdateEntity>();
            CreateMap<PlatformEntity, PlatformDomainEntity>();

            // Entity to Domain
            CreateMap<PlatformAddEntity, PlatformAddDomainEntity>();
            CreateMap<PlatformUpdateEntity, PlatformUpdateDomainEntity>();
            CreateMap<PlatformDomainEntity, PlatformEntity>();

            // Domain to Cache
            CreateMap<PlatformDomainEntity, PlatformCacheEntity>().ReverseMap();
        }
    }
}


