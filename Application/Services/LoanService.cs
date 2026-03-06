using Application.Dtos.Loan;
using Application.Dtos.LoanInstallment;
using Application.Dtos.User;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class LoanService : GenericService<Loan, LoanDto>, ILoanService
    {
        private readonly ILoanRepository loanRepository;
        private readonly ILoanInstallmentRepository loanInstallmentRepository;
        private readonly ISavingsAccountService savingsAccountService;
        private readonly IMapper mapper;

        public LoanService(ILoanRepository loanRepository, ILoanInstallmentRepository loanInstallmentRepository, ISavingsAccountService savingsAccountService, IMapper mapper)
            : base(loanRepository, mapper)
        {
            this.loanRepository = loanRepository;
            this.loanInstallmentRepository = loanInstallmentRepository;
            this.savingsAccountService = savingsAccountService;
            this.mapper = mapper;
        }

        public async Task<LoanResponseDto> CreateLoanAsync(CreateLoanDto dto)
        {
            var alreadyHasLoan = await loanRepository.HasActiveLoanAsync(dto.UserId);
            if (alreadyHasLoan)
            {
                return new LoanResponseDto
                {
                    Success = false,
                    Message = "Este cliente ya tiene un préstamo activo."
                };
            }

            var loanNumber = await GenerateUniqueLoanNumberAsync();

            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                LoanNumber = loanNumber,
                Amount = dto.Amount,
                TermMonths = dto.TermMonths,
                AnnualInterestRate = dto.AnnualInterestRate,
                CreatedAt = DateTime.UtcNow,
                Status = LoanStatus.Active,
            };

            await loanRepository.AddAsync(loan);

            var cuota = await CalculateMonthlyInstallment(dto.Amount, dto.AnnualInterestRate, dto.TermMonths);
            var installments = new List<LoanInstallment>();

            for (int i = 1; i <= dto.TermMonths; i++)
            {
                installments.Add(new LoanInstallment
                {
                    Id = Guid.NewGuid(),
                    LoanId = loan.Id,
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    Amount = cuota,
                    Status = InstallmentStatus.Pending
                });
            }

            await loanInstallmentRepository.AddRangeAsync(installments);

            var accounts = await savingsAccountService.GetAllByUserIdOrderedAsync(dto.UserId);

            if (accounts == null || !accounts.Any())
            {
                return new LoanResponseDto
                {
                    Success = false,
                    Message = "El cliente no tiene cuenta de ahorro principal registrada."
                };
            }

            var primaryAccount = accounts.FirstOrDefault(a => a.IsPrimary);
            if (primaryAccount == null)
            {
                return new LoanResponseDto
                {
                    Success = false,
                    Message = "El cliente no tiene cuenta de ahorro principal."
                };
            }

            await savingsAccountService.AddBalanceAsync(primaryAccount.Id, dto.Amount);

            return new LoanResponseDto
            {
                Success = true,
                Loan = mapper.Map<LoanDto>(loan)
            };
        }

        public async Task<UpdateInterestRateResponseDto> UpdateInterestRateAsync(EditLoanDto dto)
        {
            var loan = await loanRepository.GetById(dto.Id);
            if (loan == null)
            {
                return new UpdateInterestRateResponseDto { Success = false };
            }

            loan.AnnualInterestRate = dto.AnnualInterestRate;
            await loanRepository.UpdateAsync(loan.Id, loan);

            var installments = await loanInstallmentRepository.GetByLoanIdAsync(loan.Id);
            var now = DateTime.UtcNow;

            double r = (double)(loan.AnnualInterestRate / 12 / 100);
            double n = loan.TermMonths;
            double P = (double)loan.Amount;
            double cuotaRaw = P * (r * Math.Pow(1 + r, n)) / (Math.Pow(1 + r, n) - 1);
            decimal newCuota = Math.Round((decimal)cuotaRaw, 2);

            foreach (var installment in installments)
            {
                if (installment.DueDate > now && installment.Status == InstallmentStatus.Pending)
                {
                    installment.Amount = newCuota;
                }
            }

            await loanInstallmentRepository.UpdateRangeAsync(installments);

            return new UpdateInterestRateResponseDto
            {
                Success = true,
                LoanNumber = loan.LoanNumber,
                AnnualInterestRate = loan.AnnualInterestRate,
                NewCuota = newCuota,
                UserId = loan.UserId
            };
        }

        public async Task<List<EligibleCustomerForLoanDto>> GetEligibleCustomersForLoan(List<UserDto> customers)
        {
            var eligible = new List<EligibleCustomerForLoanDto>();

            foreach (var user in customers)
            {
                var hasLoan = await loanRepository.HasActiveLoanAsync(Guid.Parse(user.Id));
                if (hasLoan) continue;

                var debt = await loanRepository.GetTotalDebtAsync(Guid.Parse(user.Id));

                eligible.Add(new EligibleCustomerForLoanDto
                {
                    UserId = Guid.Parse(user.Id),
                    DocumentNumber = user.DocumentNumber,
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email,
                    CurrentDebt = debt
                });
            }

            return eligible.OrderByDescending(e => e.Name).ToList();
        }


        public async Task<List<LoanDisplayDto>> GetAllDisplayAsync(List<UserDto> users, string? documentNumber, string? statusFilter)
        {
            var loans = await loanRepository.GetAllQuery().ToListAsync();   

            if (!string.IsNullOrEmpty(documentNumber))
            {
                var matchingUserIds = users
                    .Where(u => u.DocumentNumber.Contains(documentNumber))
                    .Select(u => Guid.Parse(u.Id))
                    .ToList();

                loans = loans.Where(l => matchingUserIds.Contains(l.UserId)).ToList();
            }

            var installments = await loanInstallmentRepository.GetAllList();
            var result = new List<LoanDisplayDto>();

            foreach (var loan in loans)
            {
                var user = users.FirstOrDefault(u => Guid.Parse(u.Id) == loan.UserId);
                var loanInstallments = installments.Where(i => i.LoanId == loan.Id).ToList();

                var status = loanInstallments.All(i => i.Status == InstallmentStatus.Paid)
                    ? LoanStatus.Completed
                    : loanInstallments.Any(i => i.Status == InstallmentStatus.Late)
                        ? LoanStatus.Overdue
                        : LoanStatus.Active;


                if (!string.IsNullOrEmpty(statusFilter) && status.ToString() != statusFilter)
                {
                    continue;
                }

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


        public async Task<LoanDetailsDto?> GetLoanByNumberAsync(string loanNumber)
        {
            if (string.IsNullOrWhiteSpace(loanNumber))
            {
                return null;
            }

            var loan = await loanRepository.GetByNumberAsync(loanNumber);

            if (loan == null)
            {
                return null;
            }

            return mapper.Map<LoanDetailsDto>(loan);
        
        }

        public async Task<List<LoanDisplayDto>> GetActiveLoansByUserIdAsync(Guid userId)
        {
            var loans = await loanRepository.GetActiveLoansByUserIdAsync(userId);

            var loanDtos = mapper.Map<List<LoanDisplayDto>>(loans);

            foreach (var dto in loanDtos)
            {
                dto.TotalInstallments = dto.TermMonths;
                dto.PaidInstallments = await loanInstallmentRepository.CountPaidByLoanIdAsync(dto.Id);
                dto.PendingAmount = await loanInstallmentRepository.GetPendingAmountByLoanIdAsync(dto.Id);
            }

            return loanDtos;
        }


        public Task<decimal> CalculateMonthlyInstallment(decimal amount, decimal annualRate, int months)
        {
            var r = (double)annualRate / 12 / 100;
            var numerator = r * Math.Pow(1 + r, months);
            var denominator = Math.Pow(1 + r, months) - 1;
            var cuota = amount * (decimal)(numerator / denominator);
            return Task.FromResult(Math.Round(cuota, 2));
        }


        public async Task<decimal> GetAverageDebtAsync(List<UserDto> users)
        {
            var activeUsers = users.Where(u => u.IsActive).ToList();

            var totalDebt = 0m;
            var count = 0;

            foreach (var user in activeUsers)
            {
                var userId = Guid.Parse(user.Id);
                var debt = await loanRepository.GetTotalDebtAsync(userId);
                totalDebt += debt;
                count++;
            }

            return count == 0 ? 0 : totalDebt / count;
        }

        public async Task<LoanDetailsDto?> GetLoanDetailsAsync(Guid loanId)
        {
            var loan = await loanRepository.GetById(loanId);
            if (loan == null) return null;

            var installments = await loanInstallmentRepository.GetAllList();
            var loanInstallments = installments
                .Where(i => i.LoanId == loan.Id)
                .OrderBy(i => i.DueDate)
                .ToList();

            return new LoanDetailsDto
            {
                LoanId = loan.Id,
                LoanNumber = loan.LoanNumber,
                Amount = loan.Amount,
                TermMonths = loan.TermMonths,
                AnnualInterestRate = loan.AnnualInterestRate,
                InstallmentsDetails = loanInstallments.Select(i => new LoanInstallmentDetailsDto
                {
                    DueDate = i.DueDate,
                    Amount = i.Amount,
                    Status = i.Status
                }).ToList()
            };
        }


        #region private methods
        private async Task<string> GenerateUniqueLoanNumberAsync()
        {
            string number;
            do
            {
                number = new Random().Next(100000000, 999999999).ToString();
            }
            while (await loanRepository.LoanNumberExistsAsync(number));

            return number;
        }

        public async Task<int> GetActiveLoansCountAsync()
        {
            return await loanRepository.GetActiveLoansCountAsync();
        }

        public async Task<decimal> GetAverageDebtPerClientAsync()
        {
            return await loanRepository.GetAverageDebtPerClientAsync();
        }
        #endregion
    }
}
