using Application.Dtos.Loan;
using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Interfaces;
using MediatR;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Features.Loan.Commands.AssignLoan
{
    // <summary>
    // Parameters required to create a new loan
    // </summary>
    public class AssignLoanCommand : IRequest<LoanResponseDto>
    {
       // <example>1</example>
        [SwaggerParameter(Description = "The ID of the client to assign the loan")]
        public Guid UserId { get; set; }

        // <example>50000</example>
        [SwaggerParameter(Description = "The loan amount to be credited")]
        public decimal Amount { get; set; }

        // <example>24</example>
        [SwaggerParameter(Description = "Number of months for repayment")]
        public int TermMonths { get; set; }

        // <example>12</example>
        [SwaggerParameter(Description = "Annual interest rate in percentage (e.g. 12 = 12%)")]
        public decimal AnnualInterestRate { get; set; }
        [JsonIgnore]
        public UserDto? User { get; set; }
    }

    public class AssignLoanCommandHandler : IRequestHandler<AssignLoanCommand, LoanResponseDto>
    {
        private readonly ILoanRepository loanRepository;
        private readonly ILoanInstallmentRepository loanInstallmentRepository;
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly IEmailService emailService;

        public AssignLoanCommandHandler(ILoanRepository loanRepository, ILoanInstallmentRepository loanInstallmentRepository,
            ISavingsAccountRepository savingsAccountRepository, IEmailService emailService)
        {
            this.loanRepository = loanRepository;
            this.loanInstallmentRepository = loanInstallmentRepository;
            this.savingsAccountRepository = savingsAccountRepository;
            this.emailService = emailService;
        }

        public async Task<LoanResponseDto> Handle(AssignLoanCommand command, CancellationToken cancellationToken)
        {
            var alreadyHasLoan = await loanRepository.HasActiveLoanAsync(command.UserId);
            if (alreadyHasLoan)
            {
                return new LoanResponseDto
                {
                    Success = false,
                    Message = "Este cliente ya tiene un préstamo activo."
                };
            }

            var loanNumber = await GenerateUniqueLoanNumberAsync();

            var loan = new Domain.Entities.Loan
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                LoanNumber = loanNumber,
                Amount = command.Amount,
                TermMonths = command.TermMonths,
                AnnualInterestRate = command.AnnualInterestRate,
                CreatedAt = DateTime.UtcNow,
                Status = LoanStatus.Active
            };

            await loanRepository.AddAsync(loan);

            var cuota = CalculateMonthlyInstallment(command.Amount, command.AnnualInterestRate, command.TermMonths);
            var installments = new List<Domain.Entities.LoanInstallment>();

            for (int i = 1; i <= command.TermMonths; i++)
            {
                installments.Add(new Domain.Entities.LoanInstallment
                {
                    Id = Guid.NewGuid(),
                    LoanId = loan.Id,
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    Amount = cuota,
                    Status = InstallmentStatus.Pending
                });
            }

            await loanInstallmentRepository.AddRangeAsync(installments);

            var accounts = await savingsAccountRepository.GetByUserIdAsync(command.UserId);
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

            primaryAccount.Balance += command.Amount;
            await savingsAccountRepository.UpdateAsync(primaryAccount.Id, primaryAccount);

            var user = command.User;

            if (!string.IsNullOrEmpty(user.Email))
            {
                await emailService.SendAsync(new Application.Dtos.Email.EmailRequestDto
                {
                    To = user.Email,
                    Subject = "Préstamo aprobado",
                    HtmlBody = $@"
                    <p>Estimado cliente,</p>
                    <p>Su préstamo ha sido aprobado con los siguientes detalles:</p>
                    <ul>
                        <li><b>Número de préstamo:</b> {loan.LoanNumber}</li>
                        <li><b>Monto:</b> {loan.Amount:C}</li>
                        <li><b>Plazo:</b> {loan.TermMonths} meses</li>
                        <li><b>Tasa anual:</b> {loan.AnnualInterestRate}%</li>
                    </ul>
                    <p>Gracias por confiar en nosotros</p>"
                });
            }

            return new LoanResponseDto
            {
                Success = true,
                Loan = new LoanDto
                {
                    Id = loan.Id,
                    UserId = loan.UserId,
                    LoanNumber = loan.LoanNumber,
                    Amount = loan.Amount,
                    TermMonths = loan.TermMonths,
                    AnnualInterestRate = loan.AnnualInterestRate,
                    Status = loan.Status,
                    CreatedAt = loan.CreatedAt
                }
            };
        }
        #region private methods
        private decimal CalculateMonthlyInstallment(decimal amount, decimal annualRate, int termMonths)
        {
            var monthlyRate = (annualRate / 100) / 12;
            return amount * monthlyRate / (1 - (decimal)Math.Pow(1 + (double)monthlyRate, -termMonths));
        }

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
        #endregion
    }


}

