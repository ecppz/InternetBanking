using Application.Dtos.User;
using Application.Features.Users.Commands.ChangeUserStatus;
using Application.Features.Users.Commands.CreateCommerceUser;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.Commands.Update;
using Application.Features.Users.Queries.GetAllUsers;
using Application.Features.Users.Queries.GetCommerceUsers;
using Application.Features.Users.Queries.GetUserDetails;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Security.Claims;

namespace InternetBankingApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin,Commerce")]
    [Route("api/v1/user")]
    [SwaggerTag("Gestión de Usuarios del sistema")]
    public class UserController : BaseApiController
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Listado de usuarios",
            Description = "Obtiene un listado paginado de usuarios (Admin, Cajero, Cliente) excluyendo Comercios."
        )]
        public async Task<IActionResult> Get([FromQuery] GetAllUsersQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        //

        [HttpGet("Commerce")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetCommerceUsersResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
        Summary = "Listado de usuarios comercio",
        Description = "Obtiene un listado paginado de los usuarios registrados con el rol comercio."
        )]
        public async Task<IActionResult> GetCommerce([FromQuery] GetCommerceUsersQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        //
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SaveUserResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(
            Summary = "Crear nuevo usuario",
            Description = "Crea un usuario con rol Administrador, Cajero o Cliente. Si el rol es 'Cliente', se genera automáticamente su cuenta de ahorro principal con el monto inicial proporcionado."
        )]
        public async Task<IActionResult> Post([FromBody] CreateUserCommand command)
        {

            var result = await Mediator.Send(command);

            if (result.HasError)
            {

                if (result.Errors!.Any(e => e.Contains("ya existe") || e.ToLower().Contains("duplicate") || e.Contains("taken")))
                {
                    return Conflict(new { message = result.Errors });
                }

                return BadRequest(new { message = result.Errors });
            }

            return StatusCode(StatusCodes.Status201Created, result);
        }

        //

        [Authorize(Roles = "Admin")]
        [HttpPost("commerce/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SaveUserResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(SaveUserResponseDto))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Crear un nuevo commercio,",
            Description = "Crea un usuario con rol commercio se genera automáticamente su cuenta de ahorro principal con el monto inicial proporcionado."
        )]
        public async Task<IActionResult> CreateCommerce(int commerceId, [FromBody] CreateCommerceUserCommand command)
        {

            command.CommerceId = commerceId;

            var result = await Mediator.Send(command);

            if (result.HasError)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(null, new { id = result.Id }, result);
        }

        //

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(
            Summary = "Actualizar un usuario existente"
            )]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserCommand command)
        {

            command.Id = id;

            var result = await Mediator.Send(command);

            if (result.HasError)
            {
                if (result.Errors.Any(e => e.Contains("no existe"))) return NotFound();
                if (result.Errors.Any(e => e.Contains("ya está en uso"))) return Conflict(result);
                return BadRequest(result);
            }

            return NoContent();
        }

        //

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Activa o desactiva un usuario existente"
            )]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] UpdateStatusRequest body)
        {
     
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var command = new ChangeUserStatusCommand
            {
                Id = id,
                Status = body.Status,
                AdminId = adminId
            };

            var result = await Mediator.Send(command);

            if (result.HasError)
            {
                if (result.Errors.Any(e => e.Contains("propio estado")))
                    return Forbid(); // 403 Forbidden

                if (result.Errors.Any(e => e.Contains("no encontrado")))
                    return NotFound(result); // 404 Not Found

                return BadRequest(result);
            }

            return NoContent(); 
        }

        public class UpdateStatusRequest
        {
            public bool Status { get; set; }
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Busca usuarios por id"
            )]
        public async Task<IActionResult> GetDetails(string id)
        {
            var result = await Mediator.Send(new GetUserDetailsQuery { Id = id });

            if (result == null)
                return NotFound();

 
            return Ok(result);
        }

    }
}
