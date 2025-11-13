using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class LoanRepository : GenericRepository<Loan>, ILoanRepository
    {
        public LoanRepository(InternetBankingContextDB context) : base(context) { }

        public async Task<List<Loan>> GetByUserIdAsync(Guid userId)
        {
            return await context.Loans
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasActiveLoanAsync(Guid userId)
        {
            return await context.Loans.AnyAsync(l => l.UserId == userId && l.Status == LoanStatus.Active);
        }

        public async Task<bool> LoanNumberExistsAsync(string loanNumber)
        {
            return await context.Loans.AnyAsync(l => l.LoanNumber == loanNumber);
        }
        public async Task<decimal> GetTotalDebtAsync(Guid userId)
        {
            return await context.Loans
                .Where(l => l.UserId == userId && l.Status == LoanStatus.Active)
                .SumAsync(l => l.Amount);
        }
    }
}