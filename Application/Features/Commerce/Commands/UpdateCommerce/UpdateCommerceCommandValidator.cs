using FluentValidation;

namespace Application.Features.Commerce.Commands.UpdateCommerce
{
    public class UpdateCommerceCommandValidator : AbstractValidator<UpdateCommerceCommand>
    {
        public UpdateCommerceCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Commerce Id is required");

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
