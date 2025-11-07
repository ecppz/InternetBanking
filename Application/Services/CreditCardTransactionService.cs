using Application.Dtos.CreditCardTransaction;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class CreditCardTransactionService : GenericService<CreditCardTransaction, CreditCardTransactionDto>, ICreditCardTransactionService
    {
        private readonly ICreditCardTransactionRepository creditCardTransactionRepository;
        private readonly IMapper mapper;
        public CreditCardTransactionService(ICreditCardTransactionRepository creditCardTransactionRepository, IMapper mapper) : base(creditCardTransactionRepository, mapper)
        {
            this.creditCardTransactionRepository = creditCardTransactionRepository;
            this.mapper = mapper;
        }
    }
}
