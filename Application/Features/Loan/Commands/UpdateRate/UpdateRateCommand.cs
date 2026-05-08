using Application.Exceptions;
using Domain.Interfaces;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Application.Features.Loan.Commands.UpdateRate;

public class UpdateRateCommand : IRequest<Unit>
{
    [SwaggerParameter(Description = "The unique identifier of the loan to update")]
    public Guid Id { get; set; }
    [SwaggerParameter(Description = "The new annual interest rate for the loan")]
    public decimal AnnualInterestRate { get; set; }
}

public class UpdateRateCommandHandler : IRequestHandler<UpdateRateCommand, Unit>
{
    private readonly ILoanRepository _loanRepository;

    public UpdateRateCommandHandler(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public async Task<Unit> Handle(UpdateRateCommand command, CancellationToken cancellationToken)
    {
        var loan = await _loanRepository.GetById(command.Id);
        if (loan == null)
            throw new ApiException("Loan not found", (int)HttpStatusCode.NotFound);

        loan.AnnualInterestRate = command.AnnualInterestRate;
        await _loanRepository.UpdateAsync(loan.Id, loan);

        return Unit.Value;
    }
}
