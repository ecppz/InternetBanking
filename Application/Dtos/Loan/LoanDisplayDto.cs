
using Domain.Common.Enums;

namespace Application.Dtos.Loan
{
    public class LoanDisplayDto
    {
        public Guid Id { get; set; }
        public required string LoanNumber { get; set; }
        public string? CustomerFullName { get; set; }
        public string? DocumentNumber { get; set; }
        public required decimal Amount { get; set; }
        public required int TermMonths { get; set; }
        public required decimal AnnualInterestRate { get; set; }

        public required int TotalInstallments { get; set; }
        public required int PaidInstallments { get; set; }
        public required decimal PendingAmount { get; set; }
        public LoanStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
