using Application.Dtos.LoanInstallment;

namespace Application.Interfaces
{
    public interface ILoanInstallmentService : IGenericService<LoanInstallmentDto>
    {
        Task<string> RecalculateInstallmentsAsync(Guid loanId, decimal newAnnualRate);
    }
}
