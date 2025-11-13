
using Domain.Common.Enums;

namespace Application.Dtos.Loan
{
    public class LoanDto
    {
        public required Guid Id { get; set; }
        public required Guid UserId { get; set; } // fk
        public required string LoanNumber { get; set; }
        public required decimal Amount { get; set; }
        public required int TermMonths { get; set; }
        public required decimal AnnualInterestRate { get; set; }
        public required DateTime CreatedAt { get; set; }
        public LoanStatus Status { get; set; }
    }
}
