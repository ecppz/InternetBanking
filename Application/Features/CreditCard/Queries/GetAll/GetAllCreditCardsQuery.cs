using Application.Dtos.CreditCard;
using Application.Dtos.User;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CreditCard.Queries.GetAll
{
    public class GetAllCreditCardsQuery : IRequest<IList<CreditCardDisplayDto>>
    {
        public CreditCardStatus? Status { get; set; }
        public string? DocumentNumber { get; set; }
        public List<UserDto>? Users { get; set; }
    }

    public class GetAllCreditCardsQueryHandler : IRequestHandler<GetAllCreditCardsQuery, IList<CreditCardDisplayDto>>
    {
        private readonly ICreditCardRepository creditCardRepository;
        private readonly IMapper mapper;

        public GetAllCreditCardsQueryHandler(ICreditCardRepository creditCardRepository, IMapper mapper)
        {
            this.creditCardRepository = creditCardRepository;
            this.mapper = mapper;
        }

        public async Task<IList<CreditCardDisplayDto>> Handle(GetAllCreditCardsQuery query, CancellationToken cancellationToken)
        {
            var cards = await creditCardRepository.GetAllQuery().ToListAsync(cancellationToken);

            if (!string.IsNullOrEmpty(query.DocumentNumber) && query.Users != null)
            {
                var matchingUserIds = query.Users
                    .Where(u => u.DocumentNumber.Contains(query.DocumentNumber))
                    .Select(u => Guid.Parse(u.Id))
                    .ToList();

                cards = cards.Where(c => matchingUserIds.Contains(c.UserId)).ToList();
            }

            var result = new List<CreditCardDisplayDto>();

            foreach (var card in cards)
            {
                var user = query.Users?.FirstOrDefault(u => Guid.Parse(u.Id) == card.UserId);

                if (query.Status != null && card.Status != query.Status)
                    continue;

                result.Add(new CreditCardDisplayDto
                {
                    Id = card.Id,
                    CardNumber = card.CardNumber,
                    CustomerFullName = user != null ? $"{user.Name} {user.LastName}" : "",
                    DocumentNumber = user?.DocumentNumber ?? "",
                    CreditLimit = card.CreditLimit,
                    CurrentDebt = card.CurrentDebt,
                    Status = card.Status,
                    CreatedAt = card.CreatedAt
                });
            }

            return result
                .OrderByDescending(c => c.Status == CreditCardStatus.Active)
                .ThenByDescending(c => c.CreatedAt)
                .ToList();
        }
    }
}
