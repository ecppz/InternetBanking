using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Interfaces;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Users.Commands.Update
{
    public class UpdateUserCommand : IRequest<SaveUserResponseDto>
    {
        [JsonIgnore]
        public string Id { get; set; } = string.Empty;

        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public required string Cedula { get; set; }
        public required string Correo { get; set; }
        public required string Usuario { get; set; }
        public string? Contrasena { get; set; }
        public string? ConfirmarContrasena { get; set; }
        public decimal? MontoAdicional { get; set; }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, SaveUserResponseDto>
    {
        private readonly IUserAccountServiceForWebApi _userService;
        private readonly ISavingsAccountRepository _savingsAccountRepository;

        public UpdateUserCommandHandler(
            IUserAccountServiceForWebApi userService,
            ISavingsAccountRepository savingsAccountRepository)
        {
            _userService = userService;
            _savingsAccountRepository = savingsAccountRepository;
        }

        public async Task<SaveUserResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {

            var userGuid = Guid.Parse(request.Id);
            var currentUser = await _userService.GetUserById(request.Id);

            if (currentUser == null)
            {
                return new SaveUserResponseDto
                {
                    Id = request.Id,
                    Name = request.Nombre, 
                    LastName = request.Apellido, 
                    UserName = request.Usuario, 
                    Email = request.Correo,
                    DocumentNumber = request.Cedula,
                    HasError = true,
                    Errors = new List<string> { "El usuario no existe." },
                    IsVerified = false
                };
            }


            if (!string.IsNullOrEmpty(request.Contrasena) && request.Contrasena != request.ConfirmarContrasena)
            {
                return new SaveUserResponseDto
                {
                    Id = request.Id,
                    Name = request.Nombre,
                    LastName = request.Apellido,
                    UserName = request.Usuario,
                    Email = request.Correo,
                    DocumentNumber = request.Cedula,
                    HasError = true,
                    Errors = new List<string> { "Las contraseñas no coinciden." },
                    IsVerified = false
                };
            }


            var currentRoles = await _userService.GetUserRolesAsync(userGuid);
            var primaryRole = currentRoles.FirstOrDefault() ?? string.Empty;

            var saveDto = new SaveUserDto
            {
                Id = request.Id,
                Name = request.Nombre,
                LastName = request.Apellido,
                DocumentNumber = request.Cedula,
                Email = request.Correo,
                UserName = request.Usuario,
                Password = request.Contrasena ?? string.Empty,
                Role = currentRoles.FirstOrDefault() ?? string.Empty
            };

            var editResult = await _userService.EditUser(saveDto, origin: null, isApi: true);

            if (editResult.HasError)
            {
                return new SaveUserResponseDto
                {
                    Id = request.Id,
                    Name = request.Nombre,
                    LastName = request.Apellido,
                    UserName = request.Usuario,
                    Email = request.Correo,
                    DocumentNumber = request.Cedula,
                    HasError = true,
                    Errors = editResult.Errors ?? new List<string> { "Error al actualizar los datos en Identity." },
                    IsVerified = false
                };
            }

          
            if (currentRoles.Contains(Roles.Customer.ToString()) && request.MontoAdicional.HasValue && request.MontoAdicional > 0)
            {
                
                await _savingsAccountRepository.AddBalanceToPrimaryAccountAsync(userGuid, request.MontoAdicional.Value);
            }

          
            return new SaveUserResponseDto
            {
                Id = request.Id,
                Name = request.Nombre,
                LastName = request.Apellido,
                UserName = request.Usuario,
                Email = request.Correo,
                DocumentNumber = request.Cedula,
                Roles = currentRoles.ToList(),
                IsVerified = true,
                HasError = false,
                Errors = new List<string>() 
            };
        }
    }


}
