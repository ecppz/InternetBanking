using Application.Dtos.User;
using Application.Interfaces;
using Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetUserDetails
{
    public class GetUserDetailsQuery : IRequest<GetUserDetailsVm?>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class GetUserDetailsVm
    {

        public UserDto? User { get; set; }
        public string Rol { get; set; } = string.Empty;


        public string Estado => User != null && User.IsActive ? "activo" : "inactivo";

        public CuentaPrincipalDto? CuentaPrincipal { get; set; }
    }

    public class CuentaPrincipalDto
    {
        public string NumeroCuenta { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class GetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, GetUserDetailsVm?>
    {
        private readonly IUserAccountServiceForWebApi _userService;
        private readonly ISavingsAccountRepository _savingsAccountRepository;

        public GetUserDetailsQueryHandler(
            IUserAccountServiceForWebApi userService,
            ISavingsAccountRepository savingsAccountRepository)
        {
            _userService = userService;
            _savingsAccountRepository = savingsAccountRepository;
        }

        public async Task<GetUserDetailsVm?> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
        {

            var userDto = await _userService.GetUserById(request.Id);
            if (userDto == null) return null;

            var roles = await _userService.GetUserRolesAsync(Guid.Parse(request.Id));


            var primaryAccount = await _savingsAccountRepository.GetPrimaryByUserIdAsync(Guid.Parse(request.Id));

  
            return new GetUserDetailsVm
            {
                User = userDto,
                Rol = roles.FirstOrDefault() ?? "Sin Rol",
                CuentaPrincipal = primaryAccount != null ? new CuentaPrincipalDto
                {
                    NumeroCuenta = primaryAccount.AccountNumber,
                    Balance = primaryAccount.Balance
                } : null
            };
        }
    }

}
