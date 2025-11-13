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
        }
    }
}