namespace Application.Dtos.CreditCardTransaction
{
    public class CreditCardPaymentDto
    {
        public required string OriginAccountNumber { get; set; }
        public required decimal Amount { get; set; }
        public required string CreditCardNumber { get; set; }
    }
}
