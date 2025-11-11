
namespace Application.Dtos.Transaction
{
    public class DepositConfirmationDto
    {
        public required string DestinationAccountNumber { get; set; }
        public required string DestinationOwnerFullName { get; set; }
        public required decimal Amount { get; set; }
    }
}
