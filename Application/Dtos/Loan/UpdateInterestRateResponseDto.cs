namespace Application.Dtos.Loan
{
    public class UpdateInterestRateResponseDto
    {
        public Guid UserId { get; set; }
        public bool Success { get; set; }
        public string? LoanNumber { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public decimal NewCuota { get; set; }
    }
}
