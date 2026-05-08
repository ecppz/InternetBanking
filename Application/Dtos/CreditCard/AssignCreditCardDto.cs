namespace Application.Dtos.CreditCard
{
    public class AssignCreditCardDto
    {
        public Guid UserId { get; set; }
        public required Guid AdminUserId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public decimal CreditLimit { get; set; }
    }
}
