using Application.Dtos.Loan;
using Application.Dtos.LoanInstallment;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Loan.Queries.GetById
{
    public class GetByIdLoanQuery : IRequest<LoanDetailsDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetByIdLoanQueryHandler : IRequestHandler<GetByIdLoanQuery, LoanDetailsDto?>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _installmentRepository;

        public GetByIdLoanQueryHandler(
            ILoanRepository loanRepository,
            ILoanInstallmentRepository installmentRepository)
        {
            _loanRepository = loanRepository;
            _installmentRepository = installmentRepository;
        }

        public async Task<LoanDetailsDto?> Handle(GetByIdLoanQuery query, CancellationToken cancellationToken)
        {
            var loan = await _loanRepository.GetById(query.Id);
            if (loan == null) return null;

            var installments = await _installmentRepository.GetAllList();
            var loanInstallments = installments
                .Where(i => i.LoanId == loan.Id)
                .OrderBy(i => i.DueDate)
                .ToList();

            var dto = new LoanDetailsDto
            {
                LoanId = loan.Id,
                UserId = loan.UserId,
                LoanNumber = loan.LoanNumber,
                Amount = loan.Amount,
                TermMonths = loan.TermMonths,
                AnnualInterestRate = loan.AnnualInterestRate,
                Status = loan.Status,

                InstallmentsDetails = loanInstallments.Select(i => new LoanInstallmentDetailsDto
                {
                    Id = i.Id,
                    LoanId = i.LoanId,
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    Status = i.Status
                }).ToList()
            };

            return dto;
        }
    }
}
