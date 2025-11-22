using Application.Dtos.SavingsAccount;
using Application.ViewModels.SavingsAccount;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings.DtosAndViewModels
{
    public class SavingsAccountDtoMappingProfile : Profile
    {
        public SavingsAccountDtoMappingProfile()
        {
            CreateMap<SavingsAccount, SavingsAccountDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ReverseMap()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            // CreateSavingsAccountDto -> SavingsAccount
            CreateMap<CreateSavingsAccountDto, SavingsAccount>();

        // SavingsAccountDetailDto -> SavingsAccount (solo ida, las transacciones se ignoran)
            CreateMap<SavingsAccountDetailDto, SavingsAccount>().ReverseMap()
                    .ForMember(dest => dest.Transactions, opt => opt.Ignore());

        }
    }
}
