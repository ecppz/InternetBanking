using FluentValidation;

namespace Application.Features.Commerce.Commands.ChangeCommerceStatus
{
    public class ChangeCommerceStatusCommandValidator : AbstractValidator<ChangeCommerceStatusCommand>
    {
        public ChangeCommerceStatusCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Commerce Id is required");

            RuleFor(c => c.Status)
                .NotNull().WithMessage("Status must be provided");
        }
    }
}
