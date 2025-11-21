using Application.Dtos.CreditCard;
using Application.ViewModels.CreditCard;
using Application.ViewModels.CreditCardTransaction;
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

        CreateMap<CreditCardDetailsDto, CreditCardPaymentConfirmationViewModel>()
            .ForMember(dest => dest.CreditCardId, opt => opt.MapFrom(src => src.CreditCardId))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.HolderName, opt => opt.MapFrom(src => src.HolderName))
            .ForMember(dest => dest.HolderLastName, opt => opt.MapFrom(src => src.HolderLastName))
            .ForMember(dest => dest.DebtBeforePayment, opt => opt.MapFrom(src => src.CurrentDebt))
            .ForMember(dest => dest.CreditCardNumber, opt => opt.Ignore())
            .ForMember(dest => dest.OriginAccountNumber, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentAmount, opt => opt.Ignore())
            .ForMember(dest => dest.TransactionDate, opt => opt.Ignore());

    }

}
