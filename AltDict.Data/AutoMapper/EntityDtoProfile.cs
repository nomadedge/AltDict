using AltDict.Data.Dtos;
using AltDict.Data.Entities;
using AutoMapper;

namespace AltDict.Data.AutoMapper
{
    public class EntityDtoProfile : Profile
    {
        public EntityDtoProfile()
        {
            CreateMap<Connection, ConnectionDto>()
                .ReverseMap()
                ;
        }
    }
}
