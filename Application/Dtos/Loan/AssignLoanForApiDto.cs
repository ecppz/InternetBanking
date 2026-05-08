namespace Application.Dtos.Loan
{
    public class AssignLoanForApiDto
    {
        public Guid UserId { get; set; }
        public Guid AdminUserId { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
    }
}
