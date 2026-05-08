namespace Application.Dtos.CreditCard
{
    public class AssignCreditCardForApiDto
    {
        public Guid UserId { get; set; }
        public Guid AdminUserId { get; set; }
        public decimal CreditLimit { get; set; }
    }
}
