
using Domain.Common.Enums;

namespace Application.ViewModels.LoanInstallment
{
    public class LoanInstallmentViewModel
    {
        public Guid Id { get; set; }
        public Guid LoanId { get; set; }
        public required DateTime DueDate { get; set; }
        public required decimal Amount { get; set; }
        public InstallmentStatus Status { get; set; }
    }
}
