namespace Application.Dtos.CreditCardTransaction
{
    public class CreditCardPaymentDto
    {
        public required Guid OriginAccountId { get; set; }
        public required decimal Amount { get; set; }
        public required Guid CreditCardId { get; set; }
    }
}
