using Application.Dtos.CreditCardTransaction;
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
        private readonly ITransactionRepository transactionRepository;
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly IMapper mapper;
        public CreditCardTransactionService(ICreditCardTransactionRepository creditCardTransactionRepository, ICreditCardRepository creditCardRepository,
            ITransactionRepository transactionRepository, ISavingsAccountRepository savingsAccountRepository, IMapper mapper) 
            : base(creditCardTransactionRepository, mapper)
        {
            this.creditCardTransactionRepository = creditCardTransactionRepository;
            this.creditCardRepository = creditCardRepository;
            this.transactionRepository = transactionRepository;
            this.savingsAccountRepository = savingsAccountRepository;
            this.mapper = mapper;
        }


        public async Task<CreditCardTransactionDto?> RegisterPaymentAsync(CreditCardTransactionDto dto, Guid performedbyUserId)
        {
            var card = await creditCardRepository.GetById(dto.CreditCardId);
            if (card == null || card.Status != CreditCardStatus.Active)
            {
                return null;
            }

            var originAccount = await savingsAccountRepository.GetByIdAsync(dto.TransactionOrigin);
            if (originAccount == null || originAccount.Balance < dto.Amount)
            {
                return null;
            }

            var actualAmount = Math.Min(dto.Amount, card.CurrentDebt);

            originAccount.Balance -= actualAmount;
            card.CurrentDebt -= actualAmount;

            var creditCardTransaction = new CreditCardTransaction
            {
                Id = Guid.NewGuid(),
                CreditCardId = dto.CreditCardId,
                Date = DateTime.UtcNow,
                Amount = actualAmount,
                TransactionOrigin = dto.TransactionOrigin,
                Status = TransactionStatus.Approved,

            };

            await creditCardTransactionRepository.AddAsync(creditCardTransaction);

            var generalTransaction = new Transaction
            {
                OriginAccountId = originAccount.Id,
                DestinationAccountId = null, 
                Amount = actualAmount,
                Date = DateTime.UtcNow,
                Type = TransactionType.CreditCardPayment,
                Status = TransactionStatus.Approved,
                Origin = originAccount.AccountNumber,
                Beneficiary = card.CardNumber,
                PerformedByUserId = performedbyUserId,
            };

            await transactionRepository.AddAsync(generalTransaction);

            await savingsAccountRepository.UpdateAsync(originAccount.Id, originAccount);
            await creditCardRepository.UpdateAsync(card.Id, card);
            return new CreditCardTransactionDto
            {
                CreditCardId = card.Id,
                TransactionOrigin = originAccount.Id,
                Amount = actualAmount,
                Date = DateTime.UtcNow,
                Status = TransactionStatus.Approved,
                Type = CreditCardTransactionType.Payment
            };

        }

    }
}
