using Application.Dtos.Loan;
using Application.Features.Loan.Queries.GetAll;
using Application.Interfaces;
using Asp.Versioning;
using Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace InternetBankingApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Provides endpoints for viewing and updating loans")]
    public class LoanController(IUserAccountServiceForWebApi userAccountServiceForWebApi) : BaseApiController
    {
        private readonly IUserAccountServiceForWebApi userAccountServiceForWebApi = userAccountServiceForWebApi;
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<LoanDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get loan list",
            Description = "Returns a paginated list of loans filtered by status or client document number"
        )]
        public async Task<IActionResult> Get([FromQuery] string? documentNumber, [FromQuery] LoanStatus? status)
        {
            var allUsers = await userAccountServiceForWebApi.GetAllActiveUsers();

            var query = new GetAllLoansQuery
            {
                Status = status,
                DocumentNumber = documentNumber,
                Users = allUsers
            };

            var loans = await Mediator.Send(query);

            if (loans == null || loans.Count == 0)
                return NoContent();

            return Ok(loans);
        }



        //// 2. Asignar préstamo a cliente
        //[HttpPost]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status409Conflict)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[SwaggerOperation(
        //    Summary = "Assign loan to client",
        //    Description = "Creates a loan for a client, validates active loans, risk status, and generates amortization table"
        //)]
        //public async Task<IActionResult> Create([FromBody] CreateLoanCommand command)
        //{
        //    var result = await Mediator.Send(command);

        //    if (result.HasError)
        //    {
        //        if (result.ErrorType == "Conflict")
        //            return Conflict(result.Message);

        //        return BadRequest(result.Message);
        //    }

        //    return StatusCode(StatusCodes.Status201Created, result);
        //}

        //// 3. Obtener detalle de préstamo y tabla de amortización
        //[HttpGet("{id}")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoanDetailDto))]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[SwaggerOperation(
        //    Summary = "Get loan details",
        //    Description = "Returns loan details and amortization schedule"
        //)]
        //public async Task<IActionResult> GetById(Guid id)
        //{
        //    var loan = await Mediator.Send(new GetByIdLoanQuery { Id = id });

        //    if (loan == null)
        //        return NotFound();

        //    return Ok(loan);
        //}

        //// 4. Editar tasa de interés de préstamo
        //[HttpPatch("{id}/rate")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[SwaggerOperation(
        //    Summary = "Update loan interest rate",
        //    Description = "Updates the interest rate of a loan and recalculates installments"
        //)]
        //public async Task<IActionResult> UpdateRate(Guid id, [FromBody] UpdateLoanRateCommand command)
        //{
        //    if (id != command.Id)
        //        return BadRequest("The ID in the URL does not match the request body.");

        //    var result = await Mediator.Send(command);

        //    if (result.HasError)
        //        return BadRequest(result.Message);

        //    return NoContent();
        //}
    }
}
