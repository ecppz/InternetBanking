
namespace Application.ViewModels.HomeCustomerAccounts
{
    public class CreditCardSummaryViewModel
    {
        public string CardNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal Debt { get; set; }
    }
}
