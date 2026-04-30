
namespace Application.Dtos.Loan
{
    public class EditLoanDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required decimal AnnualInterestRate { get; set; }
    }
}
