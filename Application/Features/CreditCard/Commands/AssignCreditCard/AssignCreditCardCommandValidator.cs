using FluentValidation;

namespace Application.Features.CreditCard.Commands.AssignCreditCard
{
    public class AssignCreditCardCommandValidator : AbstractValidator<AssignCreditCardCommand>
    {
        public AssignCreditCardCommandValidator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(c => c.CreditLimit)
                .GreaterThan(0).WithMessage("Credit limit must be greater than 0")
                .LessThanOrEqualTo(100000).WithMessage("Credit limit exceeds maximum allowed");

            RuleFor(c => c.AdminUserId)
                .NotEmpty().WithMessage("AdminUserId is required");
        }
    }
}
