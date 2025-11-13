
namespace Application.Dtos.CreditCard
{
    public class CreateCreditCardDto
    {
        public Guid UserId { get; set; }   
        public required Guid AdminUserId { get; set; }
        public decimal CreditLimit { get; set; }

    }
}
