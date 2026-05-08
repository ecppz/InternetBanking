using Application.Exceptions;
using Domain.Common.Enums;
using Domain.Interfaces;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Application.Features.CreditCard.Commands.CancelCard
{
    public class CancelCreditCardCommand : IRequest<Unit>
    {
        [SwaggerParameter(Description = "The unique identifier of the credit card to cancel")]
        public Guid Id { get; set; }
    }

    public class CancelCreditCardCommandHandler : IRequestHandler<CancelCreditCardCommand, Unit>
    {
        private readonly ICreditCardRepository creditCardRepository;

        public CancelCreditCardCommandHandler(ICreditCardRepository creditCardRepository)
        {
            this.creditCardRepository = creditCardRepository;
        }

        public async Task<Unit> Handle(CancelCreditCardCommand command, CancellationToken cancellationToken)
        {
            var card = await creditCardRepository.GetById(command.Id);
            if (card == null)
                throw new ApiException("Credit card not found", (int)HttpStatusCode.NotFound);

            if (card.CurrentDebt > 0)
                throw new ApiException("Card cannot be cancelled while debt is pending", (int)HttpStatusCode.BadRequest);

            card.Status = CreditCardStatus.Cancelled;
            await creditCardRepository.UpdateAsync(card.Id, card);

            return Unit.Value;
        }
    }
}
