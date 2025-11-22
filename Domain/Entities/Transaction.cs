using Domain.Common.Enums;

namespace Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid OriginAccountId { get; set; }
        public Guid? DestinationAccountId { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Date { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public required string Beneficiary { get; set; }
        public required string Origin { get; set; }
        public string? Reason { get; set; }

    }

}
