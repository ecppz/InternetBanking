
namespace Application.ViewModels.LoanInstallment
{
    public class LoanInstallmentDetailsViewModel
    {
        public Guid Id { get; set; }
        public Guid LoanId { get; set; }
        public required DateTime DueDate { get; set; }
        public required decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsLate { get; set; }
    }
}
