using Domain.Interfaces;
using MediatR;
using System.Net;
using Application.Exceptions;

namespace Application.Features.Commerce.Commands.UpdateCommerce
{
    public class UpdateCommerceCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Rnc { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class UpdateCommerceCommandHandler : IRequestHandler<UpdateCommerceCommand, Unit>
    {
        private readonly ICommerceRepository commerceRepository;

        public UpdateCommerceCommandHandler(ICommerceRepository commerceRepository)
        {
            this.commerceRepository = commerceRepository;
        }

        public async Task<Unit> Handle(UpdateCommerceCommand command, CancellationToken cancellationToken)
        {
            var commerce = await commerceRepository.GetById(command.Id);
            if (commerce == null)
                throw new ApiException("Commerce not found", (int)HttpStatusCode.NotFound);

            commerce.Name = command.Name;
            commerce.Rnc = command.Rnc;
            commerce.Address = command.Address;

            await commerceRepository.UpdateAsync(commerce.Id, commerce);

            return Unit.Value;
        }
    }
}
