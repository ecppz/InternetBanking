using Application.Dtos.CreditCardTransaction.CashAdvance;
using Application.Dtos.Email;
using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class CashAdvanceService : ICashAdvanceService
    {
        private readonly ICreditCardRepository creditCardRepository;
        private readonly ICreditCardTransactionRepository creditCardTransactionRepository;
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly IEmailService emailService;
        private readonly ITransactionRepository transactionRepository;

        public CashAdvanceService(ICreditCardRepository creditCardRepository, ICreditCardTransactionRepository creditCardTransactionRepository,
            ISavingsAccountRepository savingsAccountRepository, ITransactionRepository transactionRepository, IEmailService emailService)
        {
            this.creditCardRepository = creditCardRepository;
            this.creditCardTransactionRepository = creditCardTransactionRepository;
            this.savingsAccountRepository = savingsAccountRepository;
            this.transactionRepository = transactionRepository;
            this.emailService = emailService;
        }

        public async Task<CashAdvanceDto?> ValidateAsync(CashAdvanceDto dto)
        {
            var card = await creditCardRepository.GetById(dto.CreditCardId);
            var account = await savingsAccountRepository.GetByIdAsync(dto.SavingsAccountId);

            if (card == null || card.Status != CreditCardStatus.Active)
                return null;

            if (account == null || account.Status != SavingsAccountStatus.Activa)
                return null;

            if (dto.Amount <= 0)
                return null;

            var availableCredit = card.CreditLimit - card.CurrentDebt;
            if (dto.Amount > availableCredit)
                return null;

            dto.Date = DateTime.UtcNow;
            return dto;
        }

        public async Task<CashAdvanceDto?> RegisterCashAdvanceAsync(CashAdvanceDto dto, UserDto user)
        {
            var card = await creditCardRepository.GetById(dto.CreditCardId);
            if (card == null) return null;

            var account = await savingsAccountRepository.GetByIdAsync(dto.SavingsAccountId);
            if (account == null) return null;

            var interest = dto.Amount * 0.0625m;

            account.Balance += dto.Amount;
            card.CurrentDebt += dto.Amount + interest;

            await creditCardRepository.UpdateAsync(card.Id, card);
            await savingsAccountRepository.UpdateAsync(account.Id, account);

            await transactionRepository.AddAsync(new Transaction
            {
                OriginAccountId = account.Id,
                DestinationAccountId = null,
                Amount = dto.Amount,
                Date = dto.Date,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Approved,
                Beneficiary = account.AccountNumber,
                Origin = account.AccountNumber,
            });

            await creditCardTransactionRepository.AddAsync(new CreditCardTransaction
            {
                CreditCardId = card.Id,
                Amount = dto.Amount,
                Date = dto.Date,
                TransactionOrigin = account.Id,
                Status = TransactionStatus.Approved,
                Type = CreditCardTransactionType.CashAdvance
            });


            if (user != null && card != null && account != null)
            {

                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Avance de efectivo registrado - ****{card.CardNumber.Substring(card.CardNumber.Length - 4)}",
                    HtmlBody = $@"
                <p>Estimado {user.Name},</p>
                <p>Hemos registrado correctamente su avance de efectivo con los siguientes detalles:</p>
                <ul>
                    <li><b>Número de tarjeta:</b> ****{card.CardNumber.Substring(card.CardNumber.Length - 4)}</li>
                    <li><b>Monto del avance:</b> {dto.Amount:C}</li>
                    <li><b>Cuenta destino:</b> ****{account.AccountNumber.Substring(account.AccountNumber.Length - 4)}</li>
                    <li><b>Fecha de transacción:</b> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</li>
                    <li><b>Interés aplicado:</b> {interest:C}</li>
                </ul>
                <p>Gracias por confiar en nosotros para sus operaciones financieras.</p>"
                });
            }


            return dto;
        }



    }

}
