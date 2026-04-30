using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Create
{
    public class CreateCommerceUserCommand : IRequest<RegisterResponseDto>
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

        [SwaggerParameter(Description = "ID del comercio al que se asociará")]
        public Guid CommerceId { get; set; }
    }

    public class CreateCommerceUserCommandHandler : IRequestHandler<CreateCommerceUserCommand, RegisterResponseDto>
    {
        // Inyectas también el repositorio de comercios
        private readonly ICommerceRepository _commerceRepository;
        private readonly IUserAccountServiceForWebApi _identityService;
        private readonly ISavingsAccountRepository _savingsAccountRepository;

        // EL CONSTRUCTOR ES NECESARIO PARA LA INYECCIÓN
        public CreateCommerceUserCommandHandler(
            ICommerceRepository commerceRepository,
            IUserAccountServiceForWebApi identityService,
            ISavingsAccountRepository savingsAccountRepository)
        {
            _commerceRepository = commerceRepository;
            _identityService = identityService;
            _savingsAccountRepository = savingsAccountRepository;
        }

        public async Task<RegisterResponseDto> Handle(CreateCommerceUserCommand request, CancellationToken cancellationToken)
        {
            // 1. VALIDACIÓN: ¿Existe el comercio y está libre?
            var commerce = await _commerceRepository.GetByIdAsync(request.CommerceId);

            // Suponiendo que tu entidad Commerce tiene una propiedad UserId o un booleano
            if (commerce == null || commerce.UserId != null)
            {
                return new RegisterResponseDto
                {
                    HasError = true,
                    Errors = new() { "El comercio no existe o ya tiene un usuario asociado." }
                };
            }

            // 2. REGISTRO EN IDENTITY
            var identityResponse = await _identityService.RegisterUserAsync(new SaveUserDto
            {
                Name = request.FirstName,
                LastName = request.LastName,
                DocumentNumber = request.DocumentNumber,
                Email = request.Email,
                UserName = request.UserName,
                Password = request.Password,
                Role = Roles.Commerce.ToString(), // Forzamos que sea Commerce
                CurrentBalance = request.InitialAmount ?? 0
            }, origin: "API", isApi: true);

            if (identityResponse.HasError) return identityResponse;

            // 3. CREAR CUENTA DE AHORRO (Regla del PDF para comercios)
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

            // 4. ASOCIAR EL USUARIO AL COMERCIO
            commerce.UserId = identityResponse.Id;
            await _commerceRepository.UpdateAsync(commerce, commerce.Id);

            return identityResponse;
        }

        private string GenerateAccountNumber()
        {
            return new Random().Next(100000000, 999999999).ToString();
        }
    }


}

