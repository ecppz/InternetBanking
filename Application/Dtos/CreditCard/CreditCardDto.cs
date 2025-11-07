
namespace Application.Dtos.CreditCard
{
    public class CreditCardDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // fk
        public required string CardNumber { get; set; }
        public required string ExpirationDate { get; set; }
        public required string CvcHash { get; set; }
        public required decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
        public bool IsActive { get; set; }
    }
}
