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
            // 1. Validar que las contraseñas coincidan
            if (request.Contrasena != request.ConfirmarContrasena)
            {
                return CreateErrorResponse(request, "Las contraseñas no coinciden.");
            }

            // 2. Validar unicidad del comercio (Solo un usuario por ID de comercio)
            var commerceExists = await _userService.ExistsByCommerceIdAsync(request.CommerceId);
            if (commerceExists)
            {
                return CreateErrorResponse(request, $"El comercio con ID {request.CommerceId} ya tiene un usuario asociado.");
            }

            // 3. Registrar en Identity (Pasando isApi: true para que el correo envíe el TOKEN, no un link)
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

            // Aquí el servicio de Identity debe generar el token y ponerlo en el cuerpo del correo
            var response = await _userService.RegisterUserAsync(saveDto, origin: null, isApi: true);

            if (response.HasError)
            {
                return new SaveUserResponseDto
                {
                    Id = response.Id ?? string.Empty,
                    Errors = response.Errors ?? new List<string> { "Error al registrar en Identity." },
                    HasError = true,
                    Name = request.Nombre,
                    LastName = request.Apellido,
                    UserName = request.Usuario,
                    Email = request.Correo,
                    DocumentNumber = request.Cedula,
                    IsVerified = false
                };
            }

            // 4. Crear Cuenta de Ahorro Principal para el Comercio
            try
            {
                string accountNumber = await GenerateUniqueAccountNumber();

                var newAccount = new SavingsAccount
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse(response.Id),
                    AccountNumber = accountNumber,
                    Balance = request.MontoInicial,
                    IsPrimary = true, // Requisito: Marcada como principal
                    Status = SavingsAccountStatus.Activa
                };

                await _savingsAccountRepository.AddAsync(newAccount);
            }
            catch (Exception ex)
            {
                return new SaveUserResponseDto
                {
                    Id = response.Id,
                    Errors = new List<string> { $"Usuario creado y correo enviado, pero falló la cuenta bancaria: {ex.Message}" },
                    HasError = true,
                    Name = request.Nombre,
                    LastName = request.Apellido,
                    UserName = request.Usuario,
                    Email = request.Correo,
                    DocumentNumber = request.Cedula,
                    IsVerified = false
                };
            }

            // 5. Éxito (201 Created)
            return new SaveUserResponseDto
            {
                Id = response.Id,
                Errors = new List<string>(),
                HasError = false,
                Name = request.Nombre,
                LastName = request.Apellido,
                UserName = request.Usuario,
                Email = request.Correo,
                DocumentNumber = request.Cedula,
                IsVerified = true
            };
        }

        private async Task<string> GenerateUniqueAccountNumber()
        {
            Random random = new Random();
            string number;
            bool exists;
            do
            {
                // Generar número de 9 dígitos
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
                Errors = new List<string> { error },
                HasError = true,
                Name = request.Nombre,
                LastName = request.Apellido,
                UserName = request.Usuario,
                Email = request.Correo,
                DocumentNumber = request.Cedula,
                IsVerified = false
            };
        }
    }

}