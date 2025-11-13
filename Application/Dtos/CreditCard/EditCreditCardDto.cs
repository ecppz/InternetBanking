namespace Application.Dtos.CreditCard
{
    public class EditCreditCardDto
    {
        public Guid CardId { get; set; }
        public decimal NewLimit { get; set; }
    }
}
