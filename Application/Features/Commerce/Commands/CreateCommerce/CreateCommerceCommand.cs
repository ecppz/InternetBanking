using Application.Dtos.Commerce;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Commerce.Commands.CreateCommerce
{
    public class CreateCommerceCommand : IRequest<CommerceDto>
    {
        public string Name { get; set; } = string.Empty;
        public string Rnc { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class CreateCommerceCommandHandler : IRequestHandler<CreateCommerceCommand, CommerceDto>
    {
        private readonly ICommerceRepository commerceRepository;
        private readonly IMapper mapper;

        public CreateCommerceCommandHandler(ICommerceRepository commerceRepository, IMapper mapper)
        {
            this.commerceRepository = commerceRepository;
            this.mapper = mapper;
        }

        public async Task<CommerceDto> Handle(CreateCommerceCommand command, CancellationToken cancellationToken)
        {
            var commerce = new Domain.Entities.Commerce
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
                Rnc = command.Rnc,
                Address = command.Address,
                Status = true,
                CreatedAt = DateTime.UtcNow
            };

            await commerceRepository.AddAsync(commerce);
            return mapper.Map<CommerceDto>(commerce);
        }
    }
}
