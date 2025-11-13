using Application.ViewModels.CreditCardTransaction;
using Domain.Common.Enums;

namespace Application.ViewModels.CreditCard
{
    public class CreditCardDetailsViewModel
    {
        public Guid CardId { get; set; }
        public required string CardNumber { get; set; }    
        public DateTime ExpirationDate { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
        public CreditCardStatus Status { get; set; }

        // nav property
        public List<CreditCardConsumptionViewModel> Consumptions { get; set; } = new();
    }
}
