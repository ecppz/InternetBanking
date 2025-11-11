using Application.Dtos.Transaction;
using AutoMapper;
using Domain.Entities;


namespace Application.Mappings.EntitiesAndDtos
{
    public class TransactionMappingProfile : Profile
    {
        public TransactionMappingProfile()
        {
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.OriginAccountId, opt => opt.MapFrom(src => src.OriginAccountId))
                .ForMember(dest => dest.DestinationAccountId, opt => opt.MapFrom(src => src.DestinationAccountId))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src.Origin))
                .ForMember(dest => dest.Beneficiary, opt => opt.MapFrom(src => src.Beneficiary))
                .ReverseMap();





        }
    }
}
