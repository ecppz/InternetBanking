using Application.Dtos.CreditCard;
using AutoMapper;
using Domain.Entities;

public class CreditCardMappingProfile : Profile
{
    public CreditCardMappingProfile()
    {
        CreateMap<CreditCard, CreditCardDto>().ReverseMap();

        CreateMap<CreditCard, CreateCreditCardDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.AdminUserId, opt => opt.MapFrom(src => src.AdminUserId))
            .ForMember(dest => dest.CreditLimit, opt => opt.MapFrom(src => src.CreditLimit));


        CreateMap<CreditCard, CancelCreditCardDto>()
           .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.CardLastDigits, opt => opt.MapFrom(src => src.CardNumber.Substring(src.CardNumber.Length - 4)))
           .ForMember(dest => dest.CurrentDebt, opt => opt.MapFrom(src => src.CurrentDebt));

        CreateMap<CreditCard, CreditCardDetailsDto>().ReverseMap();

    }
}
