
using Domain.Common.Enums;

namespace Application.ViewModels.Transaction
{
    public class TransactionViewModel
    {
        public Guid Id { get; set; }
        public Guid OriginAccountId { get; set; }
        public Guid? DestinationAccountId { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Date { get; set; }
        public TransactionType Type { get; set; }

        // Nuevos campos trazables
        public string Status { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;

        public string Beneficiary { get; set; } = string.Empty;

        public string VisualType { get; set; } = string.Empty;

    }
}
