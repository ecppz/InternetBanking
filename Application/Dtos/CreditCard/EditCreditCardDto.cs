namespace Application.Dtos.CreditCard
{
    public class EditCreditCardDto
    {
        public Guid CardId { get; set; }
        public Guid UserId { get; set; } // fk
        public decimal NewLimit { get; set; }
    }
}
