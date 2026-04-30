using Application.Dtos.Loan;
using Application.Dtos.User;
namespace Application.Interfaces
{
    public interface ILoanService : IGenericService<LoanDto>
    {
        Task<LoanResponseDto> CreateLoanAsync(CreateLoanDto dto, UserDto user);
        Task<UpdateInterestRateResponseDto> UpdateInterestRateAsync(EditLoanDto dto, UserDto user);
        Task<List<EligibleCustomerForLoanDto>> GetEligibleCustomersForLoan(List<UserDto> customers);
        Task<List<LoanDisplayDto>> GetAllDisplayAsync(List<UserDto> users, string? documentNumber, string? statusFilter);
        Task<LoanDetailsDto?> GetLoanByNumberAsync(string loanNumber);
        Task<List<LoanDisplayDto>> GetActiveLoansByUserIdAsync(Guid userId);
        Task<decimal> CalculateMonthlyInstallment(decimal amount, decimal annualRate, int months);
        Task<decimal> GetAverageDebtAsync(List<UserDto> users);
        Task<LoanDetailsDto?> GetLoanDetailsAsync(Guid loanId);
        Task<int> GetActiveLoansCountAsync();
        Task<decimal> GetAverageDebtPerClientAsync();

    }
}
