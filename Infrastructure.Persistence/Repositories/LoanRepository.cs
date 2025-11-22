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

        public async Task<int> GetActiveLoansCountAsync()
        {
            return await context.Loans
                .CountAsync(l => l.Status == LoanStatus.Active);
        }

        public async Task<decimal> GetAverageDebtPerClientAsync()
        {
            var activeLoans = await context.Loans
                .Where(l => l.Status == LoanStatus.Active)
                .ToListAsync();

            if (!activeLoans.Any())
                return 0;

            var totalDebt = activeLoans.Sum(l => l.Amount);
            var activeClientsCount = activeLoans.Select(l => l.UserId).Distinct().Count();

            if (activeClientsCount == 0)
                return 0;

            return totalDebt / activeClientsCount;
        }
    }
}