using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ILoanInstallmentRepository : IGenericRepository<LoanInstallment>
    {
        Task<List<LoanInstallment>> GetByLoanIdAsync(Guid loanId);
    }
}
