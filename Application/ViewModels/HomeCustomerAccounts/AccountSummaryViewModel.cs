namespace Application.ViewModels.HomeCustomerAccounts
{
    public class AccountSummaryViewModel
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = null!;
        public decimal Balance { get; set; }
        public bool IsPrimary { get; set; }
    }
}
