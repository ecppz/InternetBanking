using Application.Dtos.CreditCard;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.CreditCard.Queries.GetById
{
    public class GetByIdCreditCardQuery : IRequest<CreditCardDetailsDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetByIdCreditCardQueryHandler : IRequestHandler<GetByIdCreditCardQuery, CreditCardDetailsDto?>
    {
        private readonly ICreditCardRepository creditCardRepository;
        private readonly ICreditCardTransactionRepository creditCardTransactionRepository;
        private readonly IMapper mapper;

        public GetByIdCreditCardQueryHandler(ICreditCardRepository creditCardRepository, 
            ICreditCardTransactionRepository creditCardTransactionRepository, IMapper mapper)
        {
            this.creditCardRepository = creditCardRepository;
            this.creditCardTransactionRepository = creditCardTransactionRepository;
            this.mapper = mapper;

        }

        public async Task<CreditCardDetailsDto?> Handle(GetByIdCreditCardQuery query, CancellationToken cancellationToken)
        {
            var card = await creditCardRepository.GetById(query.Id);
            if (card == null) return null;

            var transactions = await creditCardTransactionRepository.GetByCardIdAsync(query.Id);

            var dto = mapper.Map<CreditCardDetailsDto>(card);
            dto.Consumptions = mapper.Map<List<CreditCardConsumptionDto>>(transactions);

            return dto;
        }
    }
}
