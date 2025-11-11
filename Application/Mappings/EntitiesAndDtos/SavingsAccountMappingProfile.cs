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
            CreateMap<SavingsAccount, SavingsAccountDto>().ReverseMap();

            CreateMap<CreateSavingsAccountDto, SavingsAccount>()
      .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.InitialBalance));

            // Mapeo para resumen de cuenta (listado general)
            CreateMap<SavingsAccount, SavingsAccountSummaryDto>()
       .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status == SavingsAccountStatus.Activa))
       .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Status));


            // Mapeo para detalle de cuenta (sin transacciones, se llenan en el servicio)
            CreateMap<SavingsAccount, SavingsAccountDetailDto>()
                .ForMember(dest => dest.Transactions, opt => opt.Ignore());

        }
    }
}