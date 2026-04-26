using Application.Dtos.CreditCard;
using Application.Dtos.CreditCardTransaction;
using Application.ViewModels.CreditCardTransaction;
using AutoMapper;

public class CreditCardTransactionDtoMappingProfile : Profile
{
    public CreditCardTransactionDtoMappingProfile()
    {
       
        CreateMap<CreditCardTransactionDto, CreditCardConsumptionDto>().ReverseMap();
        CreateMap<CreditCardConsumptionDto, CreditCardConsumptionViewModel>().ReverseMap();
    }
}
