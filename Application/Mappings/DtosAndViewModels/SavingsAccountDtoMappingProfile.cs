using Application.Dtos.SavingsAccount;
using Application.ViewModels.SavingsAccount;
using AutoMapper;

namespace Application.Mappings.DtosAndViewModels
{
    public class SavingsAccountDtoMappingProfile : Profile
    {
        public SavingsAccountDtoMappingProfile()
        {
            CreateMap<SavingsAccountDto, SavingsAccountViewModel>().ReverseMap();
        }
    }
}
