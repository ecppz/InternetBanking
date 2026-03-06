using Application.Dtos.CreditCardTransaction.CashAdvance;
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
        private readonly ITransactionRepository transactionRepository;

        public CashAdvanceService(ICreditCardRepository creditCardRepository, ICreditCardTransactionRepository creditCardTransactionRepository,
            ISavingsAccountRepository savingsAccountRepository, ITransactionRepository transactionRepository)
        {
            this.creditCardRepository = creditCardRepository;
            this.creditCardTransactionRepository = creditCardTransactionRepository;
            this.savingsAccountRepository = savingsAccountRepository;
            this.transactionRepository = transactionRepository;
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

        public async Task<CashAdvanceDto?> RegisterCashAdvanceAsync(CashAdvanceDto dto)
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

            return dto;
        }



    }

}
