using Application.Dtos.SavingsAccount;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;

namespace Application.Mappings.EntitiesAndDtos
{
    public class SavingsAccountMappingProfile : Profile
    {
        public SavingsAccountMappingProfile()
        {
            // Entidad ↔ DTO general
            CreateMap<SavingsAccount, SavingsAccountDto>().ReverseMap();

            // CreateSavingsAccountDto → Entidad
            CreateMap<CreateSavingsAccountDto, SavingsAccount>()
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.InitialBalance));

            // Entidad ↔ SummaryDto
            CreateMap<SavingsAccount, SavingsAccountSummaryDto>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status == SavingsAccountStatus.Activa))
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Estado));

            // Entidad ↔ DetailDto
            CreateMap<SavingsAccount, SavingsAccountDetailDto>()
                .ForMember(dest => dest.Transactions, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
