namespace Application.ViewModels.CreditCard
{
    public class EditCreditCardViewModel
    {
        public Guid CardId { get; set; }
        public Guid UserId { get; set; }
        public decimal NewLimit { get; set; }
    }
}
