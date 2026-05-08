
namespace Application.Dtos.Transaction
{
    public class WithdrawalConfirmationDto
    {
        public Guid OriginUserId;
        public required string OriginAccountNumber { get; set; }
        public string OriginOwnerFullName { get; set; } = string.Empty;
        public required decimal Amount { get; set; }
    }
}
