using Application.Dtos.CreditCard;
using Application.Dtos.CreditCardTransaction;
using AutoMapper;
using Domain.Entities;
namespace Application.Mappings.EntitiesAndDtos
{
    public class CreditCardTransactionMappingProfile : Profile
    {
        public CreditCardTransactionMappingProfile()
        {

            CreateMap<CreditCardTransaction, CreditCardTransactionDto>().ReverseMap();
            CreateMap<CreditCardTransaction, CreditCardConsumptionDto>().ReverseMap();

        }
    }
}
