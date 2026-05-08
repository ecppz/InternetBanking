using Application.Dtos.Commerce;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings.EntitiesAndDtos
{
    public class CommerceMappingProfile : Profile
    {
        public CommerceMappingProfile()
        {
            CreateMap<Commerce, CommerceDto>().ReverseMap();
        }
    }

}
