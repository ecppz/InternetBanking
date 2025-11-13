
namespace Application.Dtos.Loan
{
    public class CreateLoanDto
    {
        public Guid Id { get; set; }
        public required Guid UserId { get; set; }
        public required decimal Amount { get; set; }
        public required int TermMonths { get; set; }
        public required decimal AnnualInterestRate { get; set; }
    }
}
