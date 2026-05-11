using Application.Features.SavingsAccounts.Commands.Create;
using Application.Features.SavingsAccounts.Queries.GetAccountTrasactions;
using Application.Features.SavingsAccounts.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace InternetBankingApi.Controllers.v1
{
    [Authorize(Roles = "Admin")]
    [Route("api/v1/savings-account")]
    [ApiController]
    public class SavingsAccountController : BaseApiController
    {
        /// <summary>
        /// Obtiene el listado de cuentas de ahorro con filtros y paginación.
        /// </summary>
        /// <param name="documentNumber">Cédula del cliente</param>
        /// <param name="status">Estado (activo | cancelado)</param>
        /// <param name="type">Tipo (principal | secundaria)</param>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de la página</param>
        [HttpGet]
        [ProducesResponseType(typeof(SavingsAccountListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Obtener todos las cuentas de ahorro"
            )]
        public async Task<IActionResult> GetList(
            [FromQuery] string? documentNumber,
            [FromQuery] string? status,
            [FromQuery] string? type,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetSavingsAccountListQuery
            {
                DocumentNumber = documentNumber,
                Status = status,
                Type = type,
                Page = page,
                PageSize = pageSize
            };

            return Ok(await Mediator.Send(query));
        }

        //

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(
            Summary = "Asignar cuenta de ahorro secundaria a cliente"
            )]
        public async Task<IActionResult> CreateSecondary([FromBody] CreateSecondaryAccountCommand command)
        {
            var result = await Mediator.Send(command);

            if (result.HasError)
            {
                if (result.Error == "El cliente no existe.")
                    return BadRequest(result.Error);

                return Conflict(result.Error);
            }

            return CreatedAtAction(nameof(GetList), new { accountNumber = result.AccountNumber }, result);
        }

        //

        [HttpGet("{accountNumber}/transactions")]
        [ProducesResponseType(typeof(AccountTransactionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Obtener detalles de transacciones por cuenta"
            )]
        public async Task<IActionResult> GetTransactions(string accountNumber)
        {
            var result = await Mediator.Send(new GetAccountTransactionsQuery { AccountNumber = accountNumber });

            if (result == null)
            {
                // 404 si la cuenta no existe
                return NotFound(new { message = "Cuenta no encontrada." });
            }

            return Ok(result);
        }



    }
}
