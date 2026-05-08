using Application.Dtos.Commerce;
using Application.Exceptions;
using AutoMapper;
using Domain.Interfaces;
using MediatR;
using System.Net;

namespace Application.Features.Commerce.Queries.GetById
{
    public class GetByIdCommerceQuery : IRequest<CommerceDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetByIdCommerceQueryHandler : IRequestHandler<GetByIdCommerceQuery, CommerceDto?>
    {
        private readonly ICommerceRepository commerceRepository;
        private readonly IMapper mapper;

        public GetByIdCommerceQueryHandler(ICommerceRepository commerceRepository, IMapper mapper)
        {
            this.commerceRepository = commerceRepository;
            this.mapper = mapper;
        }

        public async Task<CommerceDto?> Handle(GetByIdCommerceQuery query, CancellationToken cancellationToken)
        {
            var commerce = await commerceRepository.GetById(query.Id);
            if (commerce == null)
                throw new ApiException("Commerce not found", (int)HttpStatusCode.NotFound);

            return mapper.Map<CommerceDto>(commerce);
        }
    }
}
