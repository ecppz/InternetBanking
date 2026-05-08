using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using MediatR;

namespace Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<IEnumerable<UserDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Role { get; set; } // administrador, cajero, cliente
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
    {
        private readonly IUserAccountServiceForWebApi _userService;

        public GetAllUsersQueryHandler(IUserAccountServiceForWebApi userService)
        {
            _userService = userService;
        }

        public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {

            var allUsers = await _userService.GetAllActiveUsers();



            var query = allUsers.AsQueryable()
                .Where(u => u.Role != Roles.Commerce.ToString());

            if (!string.IsNullOrEmpty(request.Role))
            {
                query = query.Where(u => u.Role.Equals(request.Role, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Paginacion
            return query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
        }
    }
}
