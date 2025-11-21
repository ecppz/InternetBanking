namespace Application.ViewModels.Loan
{
    public class LoanPaymentViewModel
    {
        public Guid UserId { get; set; }
        public required string OriginAccountNumber { get; set; }
        public required decimal Amount { get; set; }
        public required string LoanNumber { get; set; }
    }
}
