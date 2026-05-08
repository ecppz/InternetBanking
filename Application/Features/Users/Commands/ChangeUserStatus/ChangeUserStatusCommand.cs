using Application.Dtos.User;
using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.ChangeUserStatus
{
    public class ChangeUserStatusCommand : IRequest<SaveUserResponseDto>
    {
        public string Id { get; set; } 
        public bool Status { get; set; } 
        public string? AdminId { get; set; } 
    }

    public class ChangeUserStatusCommandHandler : IRequestHandler<ChangeUserStatusCommand, SaveUserResponseDto>
    {
        private readonly IUserAccountServiceForWebApi _userService;

        public ChangeUserStatusCommandHandler(IUserAccountServiceForWebApi userService)
        {
            _userService = userService;
        }

        public async Task<SaveUserResponseDto> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
        {

            if (request.Id == request.AdminId)
            {
                return new SaveUserResponseDto
                {
                    Id = request.Id,
                    Name = "",
                    LastName = "",
                    UserName = "",
                    Email = "",
                    DocumentNumber = "",
                    HasError = true,
                    Errors = new List<string> { "Un administrador no puede modificar su propio estado de activación." },
                    IsVerified = false
                };
            }


            var user = await _userService.GetUserById(request.Id);
            if (user == null)
            {
                return new SaveUserResponseDto
                {
                    Id = request.Id,
                    Name = "",
                    LastName = "",
                    UserName = "",
                    Email = "",
                    DocumentNumber = "",
                    HasError = true,
                    Errors = new List<string> { "Usuario no encontrado." },
                    IsVerified = false
                };
            }


            var success = await _userService.SetUserActiveStatus(request.Id, request.Status, isApi: true);

            if (!success)
            {
                return new SaveUserResponseDto
                {
                    Id = request.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    DocumentNumber = user.DocumentNumber,
                    HasError = true,
                    Errors = new List<string> { "No se pudo cambiar el estado del usuario." },
                    IsVerified = false
                };
            }

            return new SaveUserResponseDto
            {
                Id = request.Id,
                Name = user.Name,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                DocumentNumber = user.DocumentNumber,
                HasError = false,
                Errors = new List<string>()
            };
        }
    }
}
