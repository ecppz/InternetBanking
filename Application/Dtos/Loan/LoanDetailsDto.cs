
using Application.Dtos.LoanInstallment;

namespace Application.Dtos.Loan
{
    public class LoanDetailsDto
    {
        public Guid LoanId { get; set; }
        public string? LoanNumber { get; set; }
        public string? CustomerFullName { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }

        //nav property
        public List<LoanInstallmentDetailsDto> InstallmentsDetails { get; set; } = new();
    }
}
