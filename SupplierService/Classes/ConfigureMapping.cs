using AutoMapper;
using MicroServices.Caching.Model.Entities;
using SupplierService.Models.Domain;
using SupplierService.Models.Entities;

namespace SupplierService.Classes
{
    public class ConfigureMapping : Profile
    {
        public ConfigureMapping()
        {
            // Domain to Entity
            CreateMap<SupplierAddEntity, SupplierEntity>();
            CreateMap<SupplierAddDomainEntity, SupplierAddEntity>();
            CreateMap<SupplierUpdateDomainEntity, SupplierUpdateEntity>();
            CreateMap<SupplierEntity, SupplierEntity>();

            // Entity to Domain
            CreateMap<SupplierAddEntity, SupplierAddDomainEntity>();
            CreateMap<SupplierUpdateEntity, SupplierUpdateDomainEntity>();
            CreateMap<SupplierDomainEntity, SupplierEntity>();

            // Domain to Cache
            CreateMap<SupplierDomainEntity, SupplierCacheEntity>()
                .ReverseMap();

        }
    }
}


