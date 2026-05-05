using Application.Dtos.Email;
using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Features.Payments.Commands.ProcessPayment
{ 
    public class ProcessPaymentCommand : IRequest<ProcessPaymentResponse>
    {
        [JsonIgnore]
        public int? CommerceId { get; set; }
        public required string CardNumber { get; set; }
        public required string MonthExpirationCard { get; set; }
        public required string YearExpirationCard { get; set; }
        public required string CVC { get; set; }
        public decimal TransactionAmount { get; set; }
    }

    public class ProcessPaymentResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResponse>
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ICreditCardTransactionRepository _cardTransactionRepository;
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserAccountServiceForWebApi _userService;
        private readonly IEmailService _emailService;

        public ProcessPaymentCommandHandler(
            ICreditCardRepository creditCardRepository,
            ICreditCardTransactionRepository cardTransactionRepository,
            ISavingsAccountRepository savingsAccountRepository,
            ITransactionRepository transactionRepository,
            IUserAccountServiceForWebApi userService,
            IEmailService emailService)
        {
            _creditCardRepository = creditCardRepository;
            _cardTransactionRepository = cardTransactionRepository;
            _savingsAccountRepository = savingsAccountRepository;
            _transactionRepository = transactionRepository;
            _userService = userService;
            _emailService = emailService;
        }

        public async Task<ProcessPaymentResponse> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            var allUsers = await _userService.GetAllActiveUsers();
            var commerceUser = allUsers.FirstOrDefault(u => u.CommerceId == request.CommerceId);

            if (commerceUser == null)
                return new ProcessPaymentResponse { Success = false, Message = "Comercio no válido." };

            // 2. Validar Tarjeta de Crédito (Número, Expiración y CVC)
            var card = await _creditCardRepository.GetByNumberAsync(request.CardNumber);

            if (card == null || card.Status != CreditCardStatus.Active)
                return new ProcessPaymentResponse { Success = false, Message = "Tarjeta no encontrada o inactiva." };

            // Validar fecha de expiración y CVC (Asumiendo que CvcHash coincide con request.CVC)
            string expirationRequested = $"{request.MonthExpirationCard}/{request.YearExpirationCard}";
            if (card.ExpirationDate.ToString("MM/yyyy") != expirationRequested || card.CvcHash != request.CVC)
                return new ProcessPaymentResponse { Success = false, Message = "Datos de tarjeta incorrectos." };

            // 3. Validar Límite de Crédito
            if ((card.CurrentDebt + request.TransactionAmount) > card.CreditLimit)
                return new ProcessPaymentResponse { Success = false, Message = "Límite de crédito insuficiente." };

            // 4. PROCESO FINANCIERO (Transaccional)

            // A. Acreditar a la cuenta principal del Comercio
            var commerceAccount = await _savingsAccountRepository.GetPrimaryByUserIdAsync(Guid.Parse(commerceUser.Id));
            if (commerceAccount == null)
                return new ProcessPaymentResponse { Success = false, Message = "El comercio no posee cuenta principal." };

            commerceAccount.Balance += request.TransactionAmount;
            await _savingsAccountRepository.UpdateAsync(commerceAccount.Id, commerceAccount);

            // B. Registrar la transacción en el historial de la cuenta de ahorro
            await _transactionRepository.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                OriginAccountId = commerceAccount.Id,
                Amount = request.TransactionAmount,
                Date = DateTime.Now,
                Type = TransactionType.Payment,
                Status = TransactionStatus.Approved,
                Beneficiary = commerceUser.Name,
                Origin = $"Tarjeta ****{request.CardNumber.Substring(request.CardNumber.Length - 4)}",
                Reason = $"Pago recibido en {commerceUser.Name}"
            });

            // C. Cargar consumo a la Tarjeta de Crédito
            card.CurrentDebt += request.TransactionAmount;
            await _creditCardRepository.UpdateAsync(card.Id, card);

            // D. Registrar la transacción de la tarjeta
            await _cardTransactionRepository.AddAsync(new CreditCardTransaction
            {
                Id = Guid.NewGuid(),
                CreditCardId = card.Id,
                TransactionOrigin = Guid.Parse(commerceUser.Id), // ID del comercio
                Date = DateTime.Now,
                Amount = request.TransactionAmount,
                Status = TransactionStatus.Approved,
                Type = CreditCardTransactionType.Purchase
            });

            // 5. NOTIFICACIONES POR CORREO
            var clientUser = await _userService.GetUserById(card.UserId.ToString());
            string last4 = request.CardNumber.Substring(request.CardNumber.Length - 4);

            // Correo al Cliente
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = clientUser.Email,
                Subject = $"Consumo realizado con la tarjeta {last4}",
                HtmlBody = $"Se ha realizado un pago de {request.TransactionAmount} en {commerceUser.Name} el {DateTime.Now}."
            });

            // Correo al Comercio
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = commerceUser.Email,
                Subject = $"Pago recibido a través de tarjeta {last4}",
                HtmlBody = $"Has recibido un pago de {request.TransactionAmount} desde la tarjeta terminada en {last4}."
            });

            return new ProcessPaymentResponse { Success = true };
        }
    }


}
