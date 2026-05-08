using Application.Dtos.Loan;
using AutoMapper;
using Domain.Entities;
namespace Application.Mappings.EntitiesAndDtos
{
    public class LoanMappingProfile : Profile
    {
        public LoanMappingProfile()
        {
            CreateMap<Loan, LoanDto>().ReverseMap();

            CreateMap<Loan, CreateLoanDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AnnualInterestRate, opt => opt.MapFrom(src => src.AnnualInterestRate))
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.TermMonths, opt => opt.Ignore());

            CreateMap<Loan, LoanDetailsDto>()
                .ForMember(dest => dest.LoanId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.LoanNumber, opt => opt.MapFrom(src => src.LoanNumber))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount)) 
                .ForMember(dest => dest.TermMonths, opt => opt.MapFrom(src => src.TermMonths))
                .ForMember(dest => dest.AnnualInterestRate, opt => opt.MapFrom(src => src.AnnualInterestRate)) 
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))   
                .ForMember(dest => dest.HolderName, opt => opt.Ignore())
                .ForMember(dest => dest.HolderLastName, opt => opt.Ignore())
                .ForMember(dest => dest.InstallmentsDetails, opt => opt.Ignore());

            CreateMap<Loan, LoanDisplayDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.Ignore())   
                .ForMember(dest => dest.DocumentNumber, opt => opt.Ignore())   
                .ForMember(dest => dest.TotalInstallments, opt => opt.Ignore()) 
                .ForMember(dest => dest.PaidInstallments, opt => opt.Ignore())
                .ForMember(dest => dest.PendingAmount, opt => opt.Ignore());
        }
    }
}