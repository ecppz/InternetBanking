namespace Application.Dtos.CreditCard
{
    public class CancelCreditCardDto
    {
        public Guid CardId { get; set; }
        public string? CardLastDigits { get; set; }
        public decimal CurrentDebt { get; set; }
    }
}
