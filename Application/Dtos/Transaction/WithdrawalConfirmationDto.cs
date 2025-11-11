
namespace Application.Dtos.Transaction
{
    public class WithdrawalConfirmationDto
    {
        public required string OriginAccountNumber { get; set; }
        public required string OriginOwnerFullName { get; set; }
        public required decimal Amount { get; set; }
    }
}
