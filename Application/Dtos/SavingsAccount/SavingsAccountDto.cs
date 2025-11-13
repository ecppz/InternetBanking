
namespace Application.Dtos.SavingsAccount
{
    public class SavingsAccountDto
    {
        public required Guid Id { get; set; }
        public required Guid UserId { get; set; } // fk
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; }

    }
}
