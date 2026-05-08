using Domain.Common.Enums;

namespace Application.Dtos.CreditCard
{
    public class CreditCardConsumptionDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Merchant { get; set; }
        public TransactionStatus Status { get; set; }
    }
}
