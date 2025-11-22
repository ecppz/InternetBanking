
using Domain.Common.Enums;

namespace Application.ViewModels.HomeCustomerAccounts
{
    public class TransactionDetailViewModel
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = null!; // "CRÉDITO" o "DÉBITO"
        public string Origin { get; set; } = null!;
        public string Destination { get; set; } = null!;
        public TransactionStatus Status { get; set; } // "APROBADA" o "RECHAZADO"
        public string Description { get; set; } = null!;
    }
}
