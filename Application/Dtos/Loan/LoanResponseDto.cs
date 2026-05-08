namespace Application.Dtos.Loan
{
    public class LoanResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public LoanDto? Loan { get; set; }
    }

}
