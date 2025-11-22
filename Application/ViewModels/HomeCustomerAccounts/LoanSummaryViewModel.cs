using Domain.Common.Enums;


namespace Application.ViewModels.HomeCustomerAccounts
{
    public class LoanSummaryViewModel
    {
        public string LoanNumber { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public LoanStatus Status { get; set; }
    }
}
