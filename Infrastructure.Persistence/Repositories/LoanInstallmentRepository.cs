using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class LoanInstallmentRepository : GenericRepository<LoanInstallment>, ILoanInstallmentRepository
    {
        public LoanInstallmentRepository(InternetBankingContextDB context) : base(context) { }

        public async Task<List<LoanInstallment>> GetByLoanIdAsync(Guid loanId)
        {
            return await context.LoanInstallments
                .Where(i => i.LoanId == loanId)
                .ToListAsync();
        }

        public async Task<List<LoanInstallment>> GetPendingByLoanIdAsync(Guid loanId)
        {
            return await context.LoanInstallments
                .Where(i => i.LoanId == loanId && i.Status == InstallmentStatus.Pending)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }


        public async Task<int> CountPaidByLoanIdAsync(Guid loanId)
        {
            return await context.LoanInstallments
                .CountAsync(i => i.LoanId == loanId && i.Status == InstallmentStatus.Paid);
        }

        public async Task<decimal> GetPendingAmountByLoanIdAsync(Guid loanId)
        {
            return await context.LoanInstallments
                .Where(i => i.LoanId == loanId && i.Status == InstallmentStatus.Pending)
                .SumAsync(i => i.Amount);
        }

    }
}