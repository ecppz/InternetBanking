using Application.Dtos.CreditCard;
using Application.ViewModels.CreditCard;
using AutoMapper;

public class CreditCardDtoMappingProfile : Profile
{
    public CreditCardDtoMappingProfile()
    {
        CreateMap<CreditCardDto, CreditCardDisplayViewModel>().ReverseMap();
        CreateMap<CreditCardDisplayDto, CreditCardDisplayViewModel>().ReverseMap();
        
        CreateMap<CreditCardDetailsDto, CreditCardDetailsViewModel>().ReverseMap();

        CreateMap<CreditCardDto, CreditCardDetailsViewModel>();


        CreateMap<CreateCreditCardDto, CreateCreditCardViewModel>().ReverseMap();
        CreateMap<CreditCardDetailsDto, CreditCardDetailsViewModel>().ReverseMap();
        CreateMap<EditCreditCardDto, EditCreditCardViewModel>().ReverseMap();
        CreateMap<AssignCreditCardDto, AssignCreditCardViewModel>().ReverseMap();
        CreateMap<EligibleCustomerForCreditCardDto, EligibleCustomerForCreditCardViewModel>().ReverseMap();

        CreateMap<CancelCreditCardDto, CancelCreditCardViewModel>().ReverseMap();

        CreateMap<CreditCardDto, CancelCreditCardViewModel>()
            .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CardLastDigits, opt => opt.MapFrom(src => src.CardNumber.Substring(src.CardNumber.Length - 4)))
            .ForMember(dest => dest.CurrentDebt, opt => opt.MapFrom(src => src.CurrentDebt));

    }
}
