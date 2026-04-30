using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Features.Users.Commands.Create
{
    public class CreateUserCommand : IRequest<RegisterResponseDto>
    {

        [SwaggerParameter(Description = "Nombre del usuario")]

        public required string FirstName { get; set; }

        [SwaggerParameter(Description = "Apellido del usuario")]

        public required string LastName { get; set; }

        [SwaggerParameter(Description = "Numero de documento")]

        public required string DocumentNumber { get; set; }


        [SwaggerParameter(Description = "Nombre de usuario unico")]

        public required string UserName { get; set; }

        [SwaggerParameter(Description = "Correo electronico unico")]

        public required string Email { get; set; }

        [SwaggerParameter(Description = "Contraseña de la cuenta")]
        public required string Password { get; set; }

        [SwaggerParameter(Description = "Id del role")]
        public Roles Role { get; set; }

        [SwaggerParameter(Description = "Monto inicial (Solo requerido si el rol es Cliente)")]
        public decimal? InitialAmount { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, RegisterResponseDto>
    {
        private readonly IUserAccountServiceForWebApi _identityService;
        private readonly ISavingsAccountRepository _savingsAccountRepository; 

        public CreateUserCommandHandler(
            IUserAccountServiceForWebApi identityService,
            ISavingsAccountRepository savingsAccountRepository)
        {
            _identityService = identityService;
            _savingsAccountRepository = savingsAccountRepository;
        }

        public async Task<RegisterResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {

            var identityResponse = await _identityService.RegisterUserAsync(new SaveUserDto
            {
                Name = request.FirstName,
                LastName = request.LastName,
                DocumentNumber = request.DocumentNumber,
                Email = request.Email,
                UserName = request.UserName,
                Password = request.Password,
                Role = request.Role.ToString(),
                CurrentBalance = request.InitialAmount ?? 0
            }, origin: "API", isApi: true);


            if (identityResponse.HasError) return identityResponse;


            if (request.Role == Roles.Customer)
            {
                var newAccount = new SavingsAccount
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse(identityResponse.Id), 
                    AccountNumber = GenerateAccountNumber(), 
                    Balance = request.InitialAmount ?? 0,
                    IsPrimary = true,
                    Status = SavingsAccountStatus.Activa
                };

                await _savingsAccountRepository.AddAsync(newAccount);
            }

            return identityResponse;
        }

        private string GenerateAccountNumber()
        {
            return new Random().Next(100000000, 999999999).ToString();
        }
    }
}
