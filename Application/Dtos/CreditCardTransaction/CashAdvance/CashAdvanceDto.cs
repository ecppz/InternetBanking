namespace Application.Dtos.CreditCardTransaction.CashAdvance
{
    public class CashAdvanceDto
    {
        public Guid UserId { get; set; }
        public Guid CreditCardId { get; set; }
        public Guid SavingsAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
