namespace Application.ViewModels.CreditCard
{
    public class CancelCreditCardViewModel
    {
        public Guid CardId { get; set; }
        public string? CardLastDigits { get; set; } 
        public decimal CurrentDebt { get; set; }
    }
}
