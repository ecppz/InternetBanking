namespace Application.ViewModels.HomeCustomerAccounts
{
    public class CustomerHomeViewModel
    {
        public List<AccountSummaryViewModel> Accounts { get; set; } = new();
        public LoanSummaryViewModel? Loan { get; set; }
        public CreditCardSummaryViewModel? CreditCard { get; set; }
    }
}
