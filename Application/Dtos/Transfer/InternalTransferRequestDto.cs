
namespace Application.Dtos.Transfer
{
    public class InternalTransferRequestDto
    {
        public required Guid OriginAccountId { get; set; }
        public required Guid DestinationAccountId { get; set; }
        public required decimal Amount { get; set; }
    }
}
