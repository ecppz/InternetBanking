using Application.Dtos.CreditCard;
using Application.Dtos.Email;
using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Interfaces;
using MediatR;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.CreditCard.Commands.AssignCreditCard
{
    /// <summary>
    /// Parameters required to assign a new credit card
    /// </summary>
    public class AssignCreditCardCommand : IRequest<CreditCardResponseDto>
    {
        [SwaggerParameter(Description = "The ID of the client to assign the credit card")]
        public Guid UserId { get; set; }

        [SwaggerParameter(Description = "The approved credit limit for the card")]
        public decimal CreditLimit { get; set; }
        [SwaggerParameter(Description = "The ID of the admin user who assigns the card")]
        public Guid AdminUserId { get; set; }

        [JsonIgnore] 
        public UserDto? User { get; set; }
    }
    public class AssignCreditCardCommandHandler : IRequestHandler<AssignCreditCardCommand, CreditCardResponseDto>
    {
        private readonly ICreditCardRepository creditCardRepository;
        private readonly IEmailService emailService;

        public AssignCreditCardCommandHandler(ICreditCardRepository creditCardRepository, IEmailService emailService)
        {
            this.creditCardRepository = creditCardRepository;
            this.emailService = emailService;
        }

        public async Task<CreditCardResponseDto> Handle(AssignCreditCardCommand command, CancellationToken cancellationToken)
        {
            var alreadyHasCard = await creditCardRepository.HasActiveCardAsync(command.UserId);
            if (alreadyHasCard)
            {
                return new CreditCardResponseDto
                {
                    Success = false,
                    Message = "El cliente ya tiene una tarjeta activa."
                };
            }

            var cardNumber = GenerateUniqueCardNumber();
            var cvcHash = GenerateCvcHash();
            var expirationDate = DateTime.UtcNow.AddYears(3);

            var card = new Domain.Entities.CreditCard
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                CreditLimit = command.CreditLimit,
                CurrentDebt = 0,
                CardNumber = cardNumber,
                CvcHash = cvcHash,
                ExpirationDate = expirationDate,
                Status = CreditCardStatus.Active,
                AdminUserId = command.AdminUserId,
                CreatedAt = DateTime.UtcNow
            };

            await creditCardRepository.AddAsync(card);

            var user = command.User;
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = "Asignación de tarjeta de crédito",
                    HtmlBody = $@"
                        <p>Estimado {user.Name},</p>
                        <p>Se le ha asignado una nueva tarjeta de crédito.</p>
                        <ul>
                            <li><b>Número de tarjeta:</b> terminada en {card.CardNumber[^4..]}</li>
                            <li><b>Límite aprobado:</b> {card.CreditLimit:C}</li>
                            <li><b>Deuda actual:</b> {card.CurrentDebt:C}</li>
                            <li><b>Fecha de expiración:</b> {card.ExpirationDate:MM/yy}</li>
                        </ul>
                        <p>Gracias por confiar en nosotros.</p>"
                });
            }

            return new CreditCardResponseDto
            {
                Success = true,
                Message = "Tarjeta asignada correctamente.",
                CardId = card.Id,
                CardNumber = card.CardNumber[^4..],
                CreditLimit = card.CreditLimit,
                CurrentDebt = card.CurrentDebt,
                ExpirationDate = card.ExpirationDate
            };
        }

        #region private methods
        private string GenerateUniqueCardNumber()
        {
            var random = new Random();
            var number = string.Concat(Enumerable.Range(0, 16).Select(_ => random.Next(0, 10).ToString()));
            return number;
        }

        private string GenerateCvcHash()
        {
            var random = new Random();
            var cvc = random.Next(100, 999).ToString();

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(cvc);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        #endregion
    }
}
