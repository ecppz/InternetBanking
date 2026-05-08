using FluentValidation;

namespace Application.Features.Commerce.Commands.CreateCommerce
{
    public class CreateCommerceCommandValidator : AbstractValidator<CreateCommerceCommand>
    {
        public CreateCommerceCommandValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Commerce name is required")
                .MaximumLength(100).WithMessage("Commerce name must not exceed 100 characters");

            RuleFor(c => c.Rnc)
                .NotEmpty().WithMessage("RNC is required")
                .Length(9, 11).WithMessage("RNC must be between 9 and 11 characters");

            RuleFor(c => c.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(200).WithMessage("Address must not exceed 200 characters");
        }
    }
}
