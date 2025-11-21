namespace Application.ViewModels.CreditCardTransaction
{
    public class CreditCardPaymentConfirmationViewModel
    {
        public Guid CreditCardId { get; set; }
        public Guid OriginAccountId { get; set; }
        public Guid UserId { get; set; }
        public required string HolderName { get; set; }
        public required string HolderLastName { get; set; }
        public required string CreditCardNumber { get; set; }
        public required string OriginAccountNumber{ get; set; }
        public required decimal DebtBeforePayment { get; set; }
        public required decimal PaymentAmount { get; set; }
        public required DateTime TransactionDate { get; set; }
    }
}
