namespace Application.ViewModels.CreditCardTransaction
{
    public class CreditCardPaymentViewModel
    {
        public Guid UserId { get; set; }
        public required string OriginAccountNumber { get; set; }
        public required decimal Amount { get; set; }
        public required string CreditCardNumber { get; set; }
    }
}
