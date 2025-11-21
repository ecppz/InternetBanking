
using Application.Dtos.LoanInstallment;
using Domain.Common.Enums;

namespace Application.Dtos.Loan
{
    public class LoanDetailsDto
    {
        public Guid LoanId { get; set; }
        public Guid UserId { get; set; }
        public string? LoanNumber { get; set; }
        public string? CustomerFullName { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public LoanStatus Status { get; set; }
        public string HolderName { get; set; }  
        public string HolderLastName { get; set; } = string.Empty;
        //nav property
        public List<LoanInstallmentDetailsDto> InstallmentsDetails { get; set; } = new();
    }
}
