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
                .Where(i => i.LoanId == loanId && !i.IsPaid)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

    }
}