namespace Application.ViewModels.CreditCardTransaction.CashAdvance
{
    public class CashAdvanceConfirmationViewModel
    {
        public Guid UserId { get; set; }
        public Guid CreditCardId { get; set; }
        public Guid SavingsAccountId { get; set; }

        public string CreditCardNumber { get; set; } = string.Empty;
        public string SavingsAccountNumber { get; set; } = string.Empty;

        public string HolderName { get; set; } = string.Empty;
        public string HolderLastName { get; set; } = string.Empty;

        public decimal AdvanceAmount { get; set; }
        public decimal InterestApplied { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
