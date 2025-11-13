namespace Application.Dtos.Loan
{
    public class AssignLoanDto
    {
        public Guid UserId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; } 
        public required decimal Amount { get; set; }
        public required decimal AnnualRate { get; set; }
        public required int Months { get; set; }
    }
}
