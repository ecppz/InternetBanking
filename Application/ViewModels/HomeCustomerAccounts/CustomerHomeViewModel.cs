
using Application.Dtos.Loan;

namespace Application.ViewModels.HomeCustomerAccounts
{
    public class CustomerHomeViewModel
    {
        public List<AccountSummaryViewModel> Accounts { get; set; } = new();
        public LoanDisplayDto? Loan { get; set; }
        public CreditCardSummaryViewModel? CreditCard { get; set; }
    }
}
