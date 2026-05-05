using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Users.Commands.CreateCommerceUser
{
    public class CreateCommerceUserCommand : IRequest<SaveUserResponseDto>
    {
        [JsonIgnore]
        public int CommerceId { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public required string Cedula { get; set; }
        public required string Correo { get; set; }
        public required string Usuario { get; set; }
        public required string Contrasena { get; set; }
        public required string ConfirmarContrasena { get; set; }
        public decimal MontoInicial { get; set; }
    }

    public class CreateCommerceUserCommandHandler : IRequestHandler<CreateCommerceUserCommand, SaveUserResponseDto>
    {
        private readonly IUserAccountServiceForWebApi _userService;
        private readonly ISavingsAccountRepository _savingsAccountRepository;

        public CreateCommerceUserCommandHandler(
            IUserAccountServiceForWebApi userService,
            ISavingsAccountRepository savingsAccountRepository)
        {
            _userService = userService;
            _savingsAccountRepository = savingsAccountRepository;
        }

        public async Task<SaveUserResponseDto> Handle(CreateCommerceUserCommand request, CancellationToken cancellationToken)
        {
  
            if (request.Contrasena != request.ConfirmarContrasena)
                return CreateErrorResponse(request, "Las contraseñas no coinciden.");

            if (await _userService.ExistsByCommerceIdAsync(request.CommerceId))
                return CreateErrorResponse(request, $"El comercio {request.CommerceId} ya tiene un usuario.");

            if (await _userService.GetUserByUserName(request.Usuario) != null)
                return CreateErrorResponse(request, "El nombre de usuario ya existe.");

            if (await _userService.GetUserByEmail(request.Correo) != null)
                return CreateErrorResponse(request, "El correo electrónico ya existe.");


            var saveDto = new SaveUserDto
            {
                Name = request.Nombre,
                LastName = request.Apellido,
                DocumentNumber = request.Cedula,
                Email = request.Correo,
                UserName = request.Usuario,
                Password = request.Contrasena,
                Role = Roles.Commerce.ToString(),
                CommerceId = request.CommerceId
            };

            var registerResponse = await _userService.RegisterUserAsync(saveDto, origin: null, isApi: true);

            if (registerResponse.HasError)
            {
                return new SaveUserResponseDto
                {
                    Id = string.Empty, 
                    Name = request.Nombre,
                    LastName = request.Apellido,
                    UserName = request.Usuario,
                    Email = request.Correo,
                    DocumentNumber = request.Cedula,
                    HasError = true,
                    Errors = registerResponse.Errors ?? new List<string> { "Error desconocido" },
                    IsVerified = false
                };
            }

            // 3. Crear Cuenta de Ahorro
            try
            {
                string accountNumber = await GenerateUniqueAccountNumber();

                var newAccount = new SavingsAccount
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse(registerResponse.Id),
                    AccountNumber = accountNumber,
                    Balance = request.MontoInicial,
                    IsPrimary = true,
                    Status = SavingsAccountStatus.Activa
                };

                await _savingsAccountRepository.AddAsync(newAccount);
            }
            catch (Exception ex)
            {
                return new SaveUserResponseDto
                {
                    Id = registerResponse.Id,
                    Name = request.Nombre,
                    LastName = request.Apellido,
                    UserName = request.Usuario,
                    Email = request.Correo,
                    DocumentNumber = request.Cedula,
                    HasError = true,
                    Errors = new List<string> { $"Error al crear cuenta: {ex.Message}" },
                    IsVerified = false
                };
            }

   
            return new SaveUserResponseDto
            {
                Id = registerResponse.Id,
                Name = request.Nombre,
                LastName = request.Apellido,
                UserName = request.Usuario,
                Email = request.Correo,
                DocumentNumber = request.Cedula,
                HasError = false,
                Errors = new List<string>(),
                IsVerified = false 
            };
        }

        private async Task<string> GenerateUniqueAccountNumber()
        {
            Random random = new Random();
            string number;
            bool exists;
            do
            {
                number = random.Next(100000000, 1000000000).ToString();
                exists = await _savingsAccountRepository.ExistsAccountNumberAsync(number);
            } while (exists);
            return number;
        }

        private SaveUserResponseDto CreateErrorResponse(CreateCommerceUserCommand request, string error)
        {
            return new SaveUserResponseDto
            {
                Id = string.Empty, 
                Name = request.Nombre,
                LastName = request.Apellido,
                UserName = request.Usuario,
                Email = request.Correo,
                DocumentNumber = request.Cedula,
                HasError = true,
                Errors = new List<string> { error },
                IsVerified = false
            };
        }
    }

}