using Application.ViewModels.LoanInstallment;

namespace Application.ViewModels.Loan
{
    public class LoanDetailsViewModel
    {
        public Guid LoanId { get; set; }
        public string? LoanNumber { get; set; }
        public string? CustomerFullName { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }

        //nav property
        public List<LoanInstallmentDetailsViewModel> InstallmentsDetails { get; set; } = new();
    }
}
