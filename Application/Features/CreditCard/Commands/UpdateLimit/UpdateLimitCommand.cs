using Application.Exceptions;
using Domain.Interfaces;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Application.Features.CreditCard.Commands.UpdateLimit
{
    public class UpdateLimitCommand : IRequest<Unit>
    {
        [SwaggerParameter(Description = "The unique identifier of the credit card to update")]
        public Guid Id { get; set; }

        [SwaggerParameter(Description = "The new credit limit for the card")]
        public decimal NewLimit { get; set; }
    }

    public class UpdateLimitCommandHandler : IRequestHandler<UpdateLimitCommand, Unit>
    {
        private readonly ICreditCardRepository creditCardRepository;

        public UpdateLimitCommandHandler(ICreditCardRepository creditCardRepository)
        {
            this.creditCardRepository = creditCardRepository;
        }

        public async Task<Unit> Handle(UpdateLimitCommand command, CancellationToken cancellationToken)
        {
            var card = await creditCardRepository.GetById(command.Id);
            if (card == null)
                throw new ApiException("Credit card not found", (int)HttpStatusCode.NotFound);

            if (command.NewLimit < card.CurrentDebt)
                throw new ApiException("New limit cannot be less than current debt", (int)HttpStatusCode.BadRequest);

            card.CreditLimit = command.NewLimit;
            await creditCardRepository.UpdateAsync(card.Id, card);

            return Unit.Value;
        }
    }
}
