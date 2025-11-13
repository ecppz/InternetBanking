using Domain.Common.Enums;

namespace Domain.Entities
{
    public class CreditCard
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // fk
        public required Guid AdminUserId { get; set; }
        public required string CardNumber { get; set; }
        public required DateTime ExpirationDate { get; set; } 
        public required string CvcHash { get; set; } 
        public required decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
        public CreditCardStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
