using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using MediatR;

namespace Application.Features.Users.Queries.GetCommerceUsers
{
    public class GetCommerceUsersQuery : IRequest<GetCommerceUsersResponse>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }


    public class GetCommerceUsersResponse
    {
        public List<UserDto> Data { get; set; } = new();
        public PaginationInfoDto Paginacion { get; set; } = new();
    }

    public class PaginationInfoDto
    {
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalUsuarios { get; set; }
    }

    public class GetCommerceUsersQueryHandler : IRequestHandler<GetCommerceUsersQuery, GetCommerceUsersResponse>
    {
        private readonly IUserAccountServiceForWebApi _userService;

        public GetCommerceUsersQueryHandler(IUserAccountServiceForWebApi userService)
        {
            _userService = userService;
        }

        public async Task<GetCommerceUsersResponse> Handle(GetCommerceUsersQuery request, CancellationToken cancellationToken)
        {

            var allUsers = await _userService.GetAllActiveUsers();


            var commerceUsers = allUsers
                .Where(u => u.Role == Roles.Commerce.ToString())
                .OrderByDescending(u => u.Id)
                .ToList();

            var totalUsuarios = commerceUsers.Count;
            var totalPaginas = (int)Math.Ceiling(totalUsuarios / (double)request.PageSize);


            var pagedData = commerceUsers
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new GetCommerceUsersResponse
            {
                Data = pagedData,
                Paginacion = new PaginationInfoDto
                {
                    PaginaActual = request.Page,
                    TotalPaginas = totalPaginas,
                    TotalUsuarios = totalUsuarios
                }
            };
        }
    }
}
