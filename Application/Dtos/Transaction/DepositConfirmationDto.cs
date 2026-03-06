
namespace Application.Dtos.Transaction
{
    public class DepositConfirmationDto
    {
        public Guid DestinationUserId { get; set; }
        public required string DestinationAccountNumber { get; set; }
        public string DestinationOwnerFullName { get; set; } = string.Empty;
        public required decimal Amount { get; set; }
    }
}
