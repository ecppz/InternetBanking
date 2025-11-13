using Domain.Entities;
namespace Domain.Interfaces
{
    public interface ILoanRepository : IGenericRepository<Loan>
    {
        Task<bool> HasActiveLoanAsync(Guid userId);
        Task<List<Loan>> GetByUserIdAsync(Guid userId);
        Task<bool> LoanNumberExistsAsync(string loanNumber);
        Task<decimal> GetTotalDebtAsync(Guid userId);
    }
}
