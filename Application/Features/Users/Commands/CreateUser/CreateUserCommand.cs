using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<SaveUserResponseDto>
    {
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public required string Cedula { get; set; }
        public required string Correo { get; set; }
        public required string Usuario { get; set; }
        public required string Contrasena { get; set; }
        public required string ConfirmarContrasena { get; set; }
        public required string TipoUsuario { get; set; }
        public decimal? MontoInicial { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, SaveUserResponseDto>
    {
        private readonly IUserAccountServiceForWebApi _userService;
        private readonly ISavingsAccountRepository _savingsAccountRepository;

        public CreateUserCommandHandler(
            IUserAccountServiceForWebApi userService,
            ISavingsAccountRepository savingsAccountRepository)
        {
            _userService = userService;
            _savingsAccountRepository = savingsAccountRepository;
        }


        public async Task<SaveUserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
       
            if (request.Contrasena != request.ConfirmarContrasena)
            {
                return new SaveUserResponseDto
                {
                    HasError = true,
                    Errors = new List<string> { "Las contraseñas no coinciden." },
                    Id = string.Empty,
                    Name = request.Nombre,
                    LastName = request.Apellido,
                    UserName = request.Usuario,
                    Email = request.Correo,
                    DocumentNumber = request.Cedula
                };
            }


            var saveDto = new SaveUserDto
            {
                Name = request.Nombre,
                LastName = request.Apellido,
                DocumentNumber = request.Cedula,
                Email = request.Correo,
                UserName = request.Usuario,
                Password = request.Contrasena,
                Role = request.TipoUsuario,
                CurrentBalance = request.MontoInicial
            };


            var response = await _userService.RegisterUserAsync(saveDto, origin: null, isApi: true);

            if (response.HasError)
            {
                return new SaveUserResponseDto
                {
                    Id = response.Id ?? string.Empty,
                    Name = request.Nombre,
                    LastName = request.Apellido,
                    UserName = request.Usuario,
                    Email = request.Correo,
                    DocumentNumber = request.Cedula,
                    HasError = true,
                    Errors = response.Errors ?? new List<string> { "Error en el registro." }
                };
            }

            if (request.TipoUsuario.Equals(Roles.Customer.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string accountNumber = await GenerateUniqueAccountNumber();

                    var newAccount = new SavingsAccount
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.Parse(response.Id),
                        AccountNumber = accountNumber,
                        Balance = request.MontoInicial ?? 0,
                        IsPrimary = true,
                        Status = SavingsAccountStatus.Activa
                    };

                    await _savingsAccountRepository.AddAsync(newAccount);
                }
                catch (Exception ex)
                {
                    return new SaveUserResponseDto
                    {
                        Id = response.Id,
                        Name = request.Nombre,
                        LastName = request.Apellido,
                        UserName = request.Usuario,
                        Email = request.Correo,
                        DocumentNumber = request.Cedula,
                        HasError = true,
                        Errors = new List<string> { $"Usuario creado, pero falló la cuenta: {ex.InnerException?.Message ?? ex.Message}" }
                    };
                }
            }


            return new SaveUserResponseDto
            {
                Id = response.Id,
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
    }

}