
using Domain.Common.Enums;

namespace Application.Dtos.CreditCard
{
    public class CreditCardDetailsDto
    {
        public Guid CardId { get; set; }
        public required string CardNumber { get; set; }    
        public DateTime ExpirationDate { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
        public CreditCardStatus Status { get; set; }

        // nav property
        public List<CreditCardConsumptionDto> Consumptions { get; set; } = new();
    }
}
