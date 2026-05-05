using Application.Features.Payments.Commands.ProcessPayment;
using Application.Features.Payments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBankingApi.Controllers.v1
{
    [Authorize(Roles = "Admin,Commerce")]
    [ApiController]
    [Route("pay")]
    public class HermesPayController : BaseApiController
    {
        /// <summary>
        /// Obtiene un listado paginado de las transacciones registradas para un comercio.
        /// </summary>
        /// <param name="commerceId">ID del comercio (obligatorio para Admin, ignorado para Comercio)</param>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        [HttpGet("get-transactions/{commerceId}")]
        [ProducesResponseType(typeof(CommerceTransactionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTransactions(
            int commerceId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Lógica de seguridad: Si el rol es Comercio, extraemos el ID del token
            int? finalCommerceId = commerceId;

            if (User.IsInRole("Commerce"))
            {
                var commerceClaim = User.FindFirst("CommerceId")?.Value;
                if (string.IsNullOrEmpty(commerceClaim))
                    return Forbid(); // El usuario es comercio pero no tiene el Claim configurado

                finalCommerceId = int.Parse(commerceClaim);
            }

            var query = new GetCommerceTransactionsQuery
            {
                CommerceId = finalCommerceId,
                Page = page,
                PageSize = pageSize
            };

            return Ok(await Mediator.Send(query));
        }

        /// <summary>
        /// Recibe los datos de un pago para ser procesado.
        /// </summary>
        /// <param name="commerceId">ID del comercio receptor</param>
        /// <param name="command">Datos de la tarjeta y monto</param>
        [HttpPost("process-payment/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ProcessPayment(int commerceId, [FromBody] ProcessPaymentCommand command)
        {
            // Lógica de seguridad: Validar procedencia del CommerceId
            if (User.IsInRole("Commerce"))
            {
                var commerceClaim = User.FindFirst("CommerceId")?.Value;
                command.CommerceId = int.Parse(commerceClaim ?? "0");
            }
            else
            {
                command.CommerceId = commerceId;
            }

            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            // Según el mandato, la respuesta exitosa es 204 No Content
            return NoContent();
        }
    }
}
