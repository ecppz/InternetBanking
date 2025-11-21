using Application.ViewModels.LoanInstallment;
using Domain.Common.Enums;

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
        public LoanStatus Status { get; set; }
        public string HolderName { get; set; } = string.Empty;
        public string HolderLastName { get; set; } = string.Empty;
        //nav property
        public List<LoanInstallmentDetailsViewModel> InstallmentsDetails { get; set; } = new();
    }
}
