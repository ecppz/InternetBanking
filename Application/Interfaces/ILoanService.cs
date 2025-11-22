using Application.Dtos.Loan;
namespace Application.Interfaces
{
    public interface ILoanService : IGenericService<LoanDto>
    {
        Task<LoanResponseDto> CreateLoanAsync(CreateLoanDto dto);
        Task<string> UpdateInterestRateAsync(EditLoanDto dto);
        Task<List<EligibleCustomerForLoanDto>> GetEligibleCustomersForLoan();
        Task<List<LoanDisplayDto>> GetAllDisplayAsync(string? documentNumber, string? statusFilter);
        Task<LoanDetailsDto?> GetLoanByNumberAsync(string loanNumber);
        Task<List<LoanDisplayDto>> GetActiveLoansByUserIdAsync(Guid userId);

        Task<decimal> CalculateMonthlyInstallment(decimal amount, decimal annualRate, int months);
        Task<decimal> GetAverageDebtAsync();
        Task<LoanDetailsDto> GetLoanDetailsAsync(Guid loanId);

        Task<int> GetActiveLoansCountAsync();

        Task<decimal> GetAverageDebtPerClientAsync();

    }
}
