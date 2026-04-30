using Application.Dtos.Loan;
using Application.Dtos.User;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Loan.Queries.GetAll
{
    public class GetAllLoansQuery : IRequest<IList<LoanDisplayDto>>
    {
        public LoanStatus? Status { get; set; }
        public string? DocumentNumber { get; set; }
        public List<UserDto>? Users { get; set; }
    }
    public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, IList<LoanDisplayDto>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _installmentRepository;
        private readonly IMapper _mapper;

        public GetAllLoansQueryHandler(ILoanRepository loanRepository, ILoanInstallmentRepository installmentRepository,
            IMapper mapper)
        {
            _loanRepository = loanRepository;
            _installmentRepository = installmentRepository;
            _mapper = mapper;
        }

        public async Task<IList<LoanDisplayDto>> Handle(GetAllLoansQuery query, CancellationToken cancellationToken)
        {
            var loans = await _loanRepository.GetAllQuery().ToListAsync(cancellationToken);

            if (!string.IsNullOrEmpty(query.DocumentNumber) && query.Users != null)
            {
                var matchingUserIds = query.Users
                    .Where(u => u.DocumentNumber.Contains(query.DocumentNumber))
                    .Select(u => Guid.Parse(u.Id))
                    .ToList();

                loans = loans.Where(l => matchingUserIds.Contains(l.UserId)).ToList();
            }

            var installments = await _installmentRepository.GetAllList();
            var result = new List<LoanDisplayDto>();

            foreach (var loan in loans)
            {
                var user = query.Users?.FirstOrDefault(u => Guid.Parse(u.Id) == loan.UserId);
                var loanInstallments = installments.Where(i => i.LoanId == loan.Id).ToList();

                var status = loanInstallments.All(i => i.Status == InstallmentStatus.Paid)
                    ? LoanStatus.Completed
                    : loanInstallments.Any(i => i.Status == InstallmentStatus.Late)
                        ? LoanStatus.Overdue
                        : LoanStatus.Active;

                if (query.Status != null && status != query.Status)
                    continue;

                result.Add(new LoanDisplayDto
                {
                    Id = loan.Id,
                    LoanNumber = loan.LoanNumber,
                    CustomerFullName = user != null ? $"{user.Name} {user.LastName}" : "",
                    DocumentNumber = user?.DocumentNumber ?? "",
                    Amount = loan.Amount,
                    TermMonths = loan.TermMonths,
                    AnnualInterestRate = loan.AnnualInterestRate,
                    TotalInstallments = loanInstallments.Count,
                    PaidInstallments = loanInstallments.Count(i => i.Status == InstallmentStatus.Paid),
                    PendingAmount = loanInstallments
                        .Where(i => i.Status == InstallmentStatus.Pending)
                        .Sum(i => i.Amount),
                    Status = status,
                    CreatedAt = loan.CreatedAt
                });
            }

            return result
                .OrderByDescending(l => l.Status == LoanStatus.Active)
                .ThenByDescending(l => l.CreatedAt)
                .ToList();
        }
    }
}
