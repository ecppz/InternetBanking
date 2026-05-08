
using Domain.Common.Enums;

namespace Application.ViewModels.CreditCardTransaction
{
    public class CreditCardTransactionViewModel
    {
        public Guid Id { get; set; }
        public Guid CreditCardId { get; set; }
        public Guid TransactionOrigin { get; set; }
        public required DateTime Date { get; set; }
        public required decimal Amount { get; set; }
        public TransactionStatus Status { get; set; }
        public CreditCardTransactionType Type { get; set; }
    }
}
