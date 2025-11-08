using AutoMapper;
using Domain.Entities;
using Application.Dtos.SavingsAccount;
namespace Application.Mappings.EntitiesAndDtos
{
    public class SavingsAccountMappingProfile : Profile
    {
        public SavingsAccountMappingProfile()
        {
            CreateMap<SavingsAccount, SavingsAccountDto>().ReverseMap();
        }
    }
}