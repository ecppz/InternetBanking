using Application.ViewModels.LoanInstallment;
using Domain.Common.Enums;

namespace Application.ViewModels.Loan
{
    public class LoanDetailsViewModel
    {
        public Guid LoanId { get; set; }
        public string? LoanNumber { get; set; }
        public string? HolderName { get; set; }
        public string? HolderLastName { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Email { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public LoanStatus Status { get; set; }
        //nav property
        public List<LoanInstallmentDetailsViewModel> InstallmentsDetails { get; set; } = new();
    }
}
