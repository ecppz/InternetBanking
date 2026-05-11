using Application.Dtos.CreditCard;
using Application.Features.CreditCard.Commands.AssignCreditCard;
using Application.Features.CreditCard.Commands.UpdateLimit;
using Application.Features.CreditCard.Commands.CancelCard;
using Application.Features.CreditCard.Queries.GetAll;
using Application.Features.CreditCard.Queries.GetById;
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
    [Route("api/v1/credit-card")]
    [SwaggerTag("Endpoints for credit card management (requires Admin role)")]
    public class CreditCardController(IUserAccountServiceForWebApi userAccountServiceForWebApi) : BaseApiController
    {
        private readonly IUserAccountServiceForWebApi userAccountServiceForWebApi = userAccountServiceForWebApi;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CreditCardDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get credit card list",
            Description = "Returns a paginated list of credit cards filtered by client document number or status"
        )]
        public async Task<IActionResult> Get([FromQuery] string? documentNumber, [FromQuery] CreditCardStatus? status)
        {
            var allUsers = await userAccountServiceForWebApi.GetAllActiveUsers();

            var query = new GetAllCreditCardsQuery
            {
                DocumentNumber = documentNumber,
                Status = status,
                Users = allUsers
            };

            var cards = await Mediator.Send(query);

            if (cards == null || cards.Count == 0)
                return NoContent();

            return Ok(cards);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Assign credit card to client",
            Description = "Creates a credit card for an active client"
        )]
        public async Task<IActionResult> Assign([FromBody] AssignCreditCardForApiDto request)
        {
            var user = await userAccountServiceForWebApi.GetUserById(request.UserId.ToString());

            var command = new AssignCreditCardCommand
            {
                UserId = request.UserId,
                CreditLimit = request.CreditLimit,
                User = user
            };

            var result = await Mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreditCardDetailsDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get credit card details",
            Description = "Returns credit card details and associated transactions"
        )]
        public async Task<IActionResult> GetById(Guid id)
        {
            var cardDto = await Mediator.Send(new GetByIdCreditCardQuery { Id = id });

            if (cardDto == null)
                return NotFound();

            var user = await userAccountServiceForWebApi.GetUserById(cardDto.UserId.ToString());
            if (user != null)
            {
                cardDto.HolderName = user.Name;
                cardDto.HolderLastName = user.LastName;
            }

            return Ok(cardDto);
        }

        [HttpPatch("{id}/limit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Update credit card limit",
            Description = "Updates the credit limit of a card if not below current debt"
        )]
        public async Task<IActionResult> UpdateLimit([FromRoute] Guid id, [FromBody] UpdateLimitCommand command)
        {
            command.Id = id;
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Cancel credit card",
            Description = "Cancels a credit card if client has no pending debt"
        )]
        public async Task<IActionResult> Cancel([FromRoute] Guid id)
        {
            await Mediator.Send(new CancelCreditCardCommand { Id = id });
            return NoContent();
        }
    }
}
