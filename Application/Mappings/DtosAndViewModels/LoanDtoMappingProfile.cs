using Application.Dtos.Loan;
using Application.Dtos.LoanInstallment;
using Application.ViewModels.Loan;
using Application.ViewModels.LoanInstallment;
using AutoMapper;

namespace Application.Mappings.DtosAndViewModels
{
    public class LoanDtoMappingProfile : Profile
    {
        public LoanDtoMappingProfile()
        {
            CreateMap<LoanDto, LoanViewModel>().ReverseMap();
            CreateMap<LoanDisplayDto, LoanDisplayViewModel>().ReverseMap();
            CreateMap<LoanDto, EditLoanViewModel>();

            CreateMap<CreateLoanDto, CreateLoanViewModel>().ReverseMap();
  
            CreateMap<EditLoanDto, EditLoanViewModel>().ReverseMap();

            CreateMap<EligibleCustomerForLoanDto, EligibleCustomerForLoanViewModel>().ReverseMap();
            CreateMap<AssignLoanDto, AssignLoanViewModel>().ReverseMap();

            CreateMap<LoanDetailsDto, LoanDetailsViewModel>().ReverseMap();
            CreateMap<LoanInstallmentDetailsDto, LoanInstallmentDetailsViewModel>().ReverseMap();

        }
    }
}
