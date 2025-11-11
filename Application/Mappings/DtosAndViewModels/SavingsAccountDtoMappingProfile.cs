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
            // SavingsAccount <-> SavingsAccountDto
            CreateMap<SavingsAccountDto, SavingsAccount>().ReverseMap();

            // CreateSavingsAccountDto -> SavingsAccount
            CreateMap<CreateSavingsAccountDto, SavingsAccount>();

            // SavingsAccountSummaryDto <-> SavingsAccount
            CreateMap<SavingsAccount, SavingsAccountSummaryDto>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Status))
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Estado));

            // SavingsAccountDetailDto -> SavingsAccount (solo ida, las transacciones se ignoran)
            CreateMap<SavingsAccountDetailDto, SavingsAccount>().ReverseMap()
                .ForMember(dest => dest.Transactions, opt => opt.Ignore());
        }
    }
}
