using Application.Dtos.Loan;
using Application.Dtos.Transaction;
using Application.Dtos.User;

namespace Application.Interfaces
{
    public interface ILoanPaymentService
    {
        Task<TransactionDto?> RegisterPaymentAsync(LoanPaymentDto dto, Guid uperformedbyUserIdserId, UserDto user);
    }
}
