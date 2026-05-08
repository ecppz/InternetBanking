using Application.ViewModels.CreditCardTransaction;
using Domain.Common.Enums;

namespace Application.ViewModels.CreditCard
{
    public class CreditCardDetailsViewModel
    {
        public Guid CreditCardId { get; set; }
        public required string CardNumber { get; set; }    
        public DateTime ExpirationDate { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
        public CreditCardStatus Status { get; set; }
        public string HolderName { get; set; } = string.Empty;
        public string HolderLastName { get; set; } = string.Empty;

        // nav property
        public List<CreditCardConsumptionViewModel> Consumptions { get; set; } = new();
    }
}
