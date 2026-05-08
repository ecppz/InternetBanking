using FluentValidation;

namespace Application.Features.Loan.Commands.UpdateRate
{
    public class UpdateRateCommandValidator : AbstractValidator<UpdateRateCommand>
    {
        public UpdateRateCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Loan Id is required");

            RuleFor(c => c.AnnualInterestRate)
                .GreaterThan(0).WithMessage("Annual interest rate must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Annual interest rate must be less than or equal to 100");
        }
    }
}
