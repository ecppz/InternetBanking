using Application.Dtos.Commerce;
using AutoMapper;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Commerce.Queries.GetAll
{
    public class GetAllCommercesQuery : IRequest<IList<CommerceDto>>
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }

    public class GetAllCommercesQueryHandler : IRequestHandler<GetAllCommercesQuery, IList<CommerceDto>>
    {
        private readonly ICommerceRepository commerceRepository;
        private readonly IMapper mapper;

        public GetAllCommercesQueryHandler(ICommerceRepository commerceRepository, IMapper mapper)
        {
            this.commerceRepository = commerceRepository;
            this.mapper = mapper;
        }

        public async Task<IList<CommerceDto>> Handle(GetAllCommercesQuery query, CancellationToken cancellationToken)
        {
            var commercesQuery = commerceRepository.GetAllQuery()
                .Where(c => c.Status == true);

            if (query.Page.HasValue && query.PageSize.HasValue)
            {
                commercesQuery = commercesQuery
                    .Skip((query.Page.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .OrderByDescending(c => c.CreatedAt);
            }
            else
            {
                commercesQuery = commercesQuery.OrderByDescending(c => c.CreatedAt);
            }


            var commerces = await commercesQuery.ToListAsync(cancellationToken);

            return mapper.Map<IList<CommerceDto>>(commerces);
        }



    }
}
