using Application.Dtos.CreditCard;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class CreditCardService : GenericService<CreditCard, CreditCardDto>, ICreditCardService
    {
        private readonly ICreditCardRepository creditCardRepository;
        private readonly IMapper mapper;
        public CreditCardService(ICreditCardRepository creditCardRepository, IMapper mapper) : base(creditCardRepository, mapper)
        {
            this.creditCardRepository = creditCardRepository;
            this.mapper = mapper;
        }
    }
}