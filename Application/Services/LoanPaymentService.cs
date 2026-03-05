using Application.Dtos.Email;
using Application.Dtos.Loan;
using Application.Dtos.Transaction;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class LoanPaymentService : ILoanPaymentService
    {
        private readonly ILoanRepository loanRepository;
        private readonly ILoanInstallmentRepository loanInstallmentRepository;
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IMapper mapper;

        public LoanPaymentService(ILoanRepository loanRepository, ILoanInstallmentRepository loanInstallmentRepository,
            ISavingsAccountRepository savingsAccountRepository, ITransactionRepository transactionRepository, IMapper mapper)
        {
            this.loanRepository = loanRepository;
            this.loanInstallmentRepository = loanInstallmentRepository;
            this.savingsAccountRepository = savingsAccountRepository;
            this.transactionRepository = transactionRepository;
            this.mapper = mapper;
        }
        public async Task<TransactionDto?> RegisterPaymentAsync(LoanPaymentDto dto, Guid performedbyUserId)
        {
            var loan = await loanRepository.GetByNumberAsync(dto.LoanNumber); 
            if (loan == null || loan.Status == LoanStatus.Completed)
            {
                return null;
            }

            var account = await savingsAccountRepository.GetByAccountNumberAsync(dto.OriginAccountNumber);
            if (account == null || account.Status != SavingsAccountStatus.Activa)
                return null;

            if (account.Balance < dto.Amount)
                return null;

            var installments = await loanInstallmentRepository.GetByLoanIdAsync(loan.Id);
            var remaining = dto.Amount;

            foreach (var installment in installments.OrderBy(i => i.DueDate))
            {
                if (remaining <= 0) break;

                if (installment.Status == InstallmentStatus.Pending)
                {
                    if (remaining >= installment.Amount)
                    {
                        remaining -= installment.Amount;
                        installment.Status = InstallmentStatus.Paid;
                        await loanInstallmentRepository.UpdateAsync(installment.Id, installment);
                    }
                    else
                    {
                        break;
                    }
                }
            }


            account.Balance -= dto.Amount;
            if (remaining > 0)
                account.Balance += remaining;

            await savingsAccountRepository.UpdateAsync(account.Id, account);

            var transaction = new Transaction
            {
                OriginAccountId = account.Id,
                DestinationAccountId = null,
                Amount = dto.Amount,
                Date = DateTime.UtcNow,
                Type = TransactionType.LoanPayment,
                Status = TransactionStatus.Approved,
                Origin = dto.OriginAccountNumber,
                Beneficiary = loan.LoanNumber,
                PerformedByUserId = performedbyUserId,
            };

            await transactionRepository.AddAsync(transaction);

            var transactionDto = mapper.Map<TransactionDto>(transaction);

            var saldoPendiente = installments
                  .Where(i => i.Status == InstallmentStatus.Pending)
                  .Sum(i => i.Amount);

            return transactionDto;
        }
    }
}

