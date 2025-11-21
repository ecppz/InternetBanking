
namespace Application.Dtos.Loan
{
    public class LoanPaymentDto
    {
        public Guid UserId { get; set; }
        public required string OriginAccountNumber { get; set; }
        public required decimal Amount { get; set; }
        public required string LoanNumber { get; set; }
    }
}
