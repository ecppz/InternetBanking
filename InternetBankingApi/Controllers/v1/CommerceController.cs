using Application.Dtos.Commerce;
using Application.Features.Commerce.Commands.CreateCommerce;
using Application.Features.Commerce.Commands.UpdateCommerce;
using Application.Features.Commerce.Commands.ChangeCommerceStatus;
using Application.Features.Commerce.Queries.GetAll;
using Application.Features.Commerce.Queries.GetById;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace InternetBankingApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [Route("api/v1/commerce")]
    [SwaggerTag("Endpoints for commerce management (requires Admin role)")]
    public class CommerceController : BaseApiController
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CommerceDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get commerce list",
            Description = "Returns a paginated list of commerces ordered by creation date"
        )]
        public async Task<IActionResult> Get([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var query = new GetAllCommercesQuery
            {
                Page = page,
                PageSize = pageSize
            };

            var commerces = await Mediator.Send(query);

            if (commerces == null || commerces.Count == 0)
                return NoContent();

            return Ok(commerces);
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CommerceDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get commerce details",
            Description = "Returns commerce details by its unique identifier"
        )]
        public async Task<IActionResult> GetById(Guid id)
        {
            var commerce = await Mediator.Send(new GetByIdCommerceQuery { Id = id });

            if (commerce == null)
                return NotFound();

            return Ok(commerce);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Create new commerce",
            Description = "Creates a new commerce in the system"
        )]
        public async Task<IActionResult> Create([FromBody] CreateCommerceCommand command)
        {
            var result = await Mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Update commerce",
            Description = "Updates an existing commerce by its unique identifier"
        )]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommerceCommand command)
        {
            command.Id = id;
            await Mediator.Send(command);
            return NoContent();
        }
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Change commerce status",
            Description = "Activates or deactivates a commerce by its unique identifier"
        )]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeCommerceStatusCommand command)
        {
            command.Id = id;
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
