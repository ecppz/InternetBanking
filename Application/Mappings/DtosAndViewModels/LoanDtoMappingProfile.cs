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

            CreateMap<LoanDetailsDto, LoanPaymentConfirmationViewModel>()
                .ForMember(dest => dest.LoanId, opt => opt.MapFrom(src => src.LoanId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.HolderName, opt => opt.MapFrom(src => src.HolderName))
                .ForMember(dest => dest.HolderLastName, opt => opt.MapFrom(src => src.HolderLastName))
                .ForMember(dest => dest.LoanNumber, opt => opt.MapFrom(src => src.LoanNumber))
                .ForMember(dest => dest.TransactionDate, opt => opt.Ignore()) 
                .ForMember(dest => dest.OriginAccountId, opt => opt.Ignore())
                .ForMember(dest => dest.OriginAccountNumber, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentAmount, opt => opt.Ignore());



        }
    }
}
