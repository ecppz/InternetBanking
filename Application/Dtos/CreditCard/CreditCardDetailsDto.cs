
using Domain.Common.Enums;

namespace Application.Dtos.CreditCard
{
    public class CreditCardDetailsDto
    {
        public Guid CreditCardId { get; set; }
        public Guid UserId { get; set; }
        public required string CardNumber { get; set; }    
        public DateTime ExpirationDate { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
        public CreditCardStatus Status { get; set; }
        public string HolderName { get; set; } = string.Empty;
        public string HolderLastName { get; set; } = string.Empty;

        // nav property
        public List<CreditCardConsumptionDto> Consumptions { get; set; } = new();
    }
}
