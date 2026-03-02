namespace Application.ViewModels.CreditCard
{
    public class CancelCreditCardViewModel
    {
        public Guid CardId { get; set; }
        public Guid UserId { get; set; } // fk
        public string? CardLastDigits { get; set; } 
        public decimal CurrentDebt { get; set; }
    }
}
