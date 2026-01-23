using Application.Dtos.Loan;
using Application.Dtos.Transaction;

namespace Application.Interfaces
{
    public interface ILoanPaymentService
    {
        Task<TransactionDto?> RegisterPaymentAsync(LoanPaymentDto dto, Guid userId);
    }
}
