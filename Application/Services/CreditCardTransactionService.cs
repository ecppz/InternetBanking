using Application.Dtos.CreditCardTransaction;
using Application.Dtos.Email;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class CreditCardTransactionService : GenericService<CreditCardTransaction, CreditCardTransactionDto>, ICreditCardTransactionService
    {
        private readonly ICreditCardTransactionRepository creditCardTransactionRepository;
        private readonly ICreditCardRepository creditCardRepository;
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly IUserAccountService userAccountService;
        private readonly IEmailService emailService;
        private readonly IMapper mapper;
        public CreditCardTransactionService(ICreditCardTransactionRepository creditCardTransactionRepository, ICreditCardRepository creditCardRepository, 
            ISavingsAccountRepository savingsAccountRepository, IUserAccountService userAccountService, IEmailService emailService, IMapper mapper) 
            : base(creditCardTransactionRepository, mapper)
        {
            this.creditCardTransactionRepository = creditCardTransactionRepository;
            this.creditCardRepository = creditCardRepository;
            this.savingsAccountRepository = savingsAccountRepository;
            this.userAccountService = userAccountService;
            this.emailService = emailService;
            this.mapper = mapper;
        }

        public async Task<CreditCardTransactionDto?> RegisterPaymentAsync(CreditCardTransactionDto dto)
        {
            var card = await creditCardRepository.GetById(dto.CreditCardId);
            if (card == null || card.Status != CreditCardStatus.Active)
            {
                return null;
            }

            var originAccount = await savingsAccountRepository.GetByAccountNumberAsync(dto.TransactionOrigin);
            if (originAccount == null || originAccount.Balance < dto.Amount)
            {
                return null;
            }

            var actualAmount = Math.Min(dto.Amount, card.CurrentDebt);

            originAccount.Balance -= actualAmount;
            card.CurrentDebt -= actualAmount;

            var transaction = new CreditCardTransaction
            {
                Id = Guid.NewGuid(),
                CreditCardId = dto.CreditCardId,
                Date = DateTime.UtcNow,
                Amount = actualAmount,
                TransactionOrigin = dto.TransactionOrigin,
                Status = TransactionStatus.Approved
            };

            await creditCardTransactionRepository.AddAsync(transaction);

            await savingsAccountRepository.UpdateAsync(originAccount.Id, originAccount);
            await creditCardRepository.UpdateAsync(card.Id, card);

            var user = await userAccountService.GetUserById(card.UserId.ToString());
            if (user != null)
            {
                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Pago realizado a la tarjeta ****{card.CardNumber.Substring(card.CardNumber.Length - 4)}",
                    HtmlBody = $@"
                        <p>Estimado {user.Name},</p>
                        <p>Se ha realizado un pago a su tarjeta de crédito.</p>
                        <ul>
                            <li><b>Monto:</b> {actualAmount:C}</li>
                            <li><b>Cuenta origen:</b> ****{originAccount.AccountNumber.Substring(originAccount.AccountNumber.Length - 4)}</li>
                            <li><b>Tarjeta destino:</b> ****{card.CardNumber.Substring(card.CardNumber.Length - 4)}</li>
                            <li><b>Fecha:</b> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</li>
                        </ul>
                        <p>Gracias por confiar en nosotros.</p>"
                });
            }

            return mapper.Map<CreditCardTransactionDto>(transaction);
        }

    }
}
