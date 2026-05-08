using FluentValidation;

namespace Application.Features.Loan.Commands.AssignLoan
{
    public class AssignLoanCommandValidator : AbstractValidator<AssignLoanCommand>
    {
        public AssignLoanCommandValidator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(c => c.Amount)
                .GreaterThan(0).WithMessage("Loan amount must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Loan amount exceeds maximum allowed");

            RuleFor(c => c.TermMonths)
                .GreaterThan(0).WithMessage("Term must be at least 1 month")
                .LessThanOrEqualTo(360).WithMessage("Term cannot exceed 30 years");

            RuleFor(c => c.AnnualInterestRate)
                .GreaterThan(0).WithMessage("Annual interest rate must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Annual interest rate must be less than or equal to 100");
        }
    }
}
