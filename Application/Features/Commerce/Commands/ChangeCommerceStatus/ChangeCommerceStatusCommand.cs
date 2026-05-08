using Domain.Interfaces;
using MediatR;
using System.Net;
using Application.Exceptions;

namespace Application.Features.Commerce.Commands.ChangeCommerceStatus
{
    public class ChangeCommerceStatusCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public bool Status { get; set; }
    }

    public class ChangeCommerceStatusCommandHandler : IRequestHandler<ChangeCommerceStatusCommand, Unit>
    {
        private readonly ICommerceRepository commerceRepository;

        public ChangeCommerceStatusCommandHandler(ICommerceRepository commerceRepository)
        {
            this.commerceRepository = commerceRepository;
        }

        public async Task<Unit> Handle(ChangeCommerceStatusCommand command, CancellationToken cancellationToken)
        {
            var commerce = await commerceRepository.GetById(command.Id);
            if (commerce == null)
                throw new ApiException("Commerce not found", (int)HttpStatusCode.NotFound);

            commerce.Status = command.Status;

            await commerceRepository.UpdateAsync(commerce.Id, commerce);
            return Unit.Value;
        }
    }
}
