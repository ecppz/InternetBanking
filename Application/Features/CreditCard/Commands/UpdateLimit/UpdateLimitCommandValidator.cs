using FluentValidation;

namespace Application.Features.CreditCard.Commands.UpdateLimit
{
    public class UpdateLimitCommandValidator : AbstractValidator<UpdateLimitCommand>
    {
        public UpdateLimitCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Credit card Id is required");

            RuleFor(c => c.NewLimit)
                .GreaterThan(0).WithMessage("New limit must be greater than 0")
                .LessThanOrEqualTo(100000).WithMessage("New limit exceeds maximum allowed");
        }
    }
}
