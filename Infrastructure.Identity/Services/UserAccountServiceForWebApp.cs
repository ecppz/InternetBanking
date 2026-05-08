using Application.Dtos.User;
using Application.Interfaces;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Services
{
    public class UserAccountServiceForWebApp : BaseUserAccountService, IUserAccountServiceForWebApp
    {
        private readonly UserManager<UserAccount> userManager;
        private readonly SignInManager<UserAccount> signInManager;
        public UserAccountServiceForWebApp(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, IEmailService emailService)
               : base(userManager, emailService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            var response = new LoginResponseDto
            {
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                Email = "",
                DocumentNumber = "",
                HasError = false,
                Errors = [],
                Roles = [],
                IsVerified = false
            };

            var user = await userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No hay cuenta registrada con el usuario: {loginDto.UserName}");
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Errors.Add($"La cuenta {loginDto.UserName} no está confirmada. Revisa tu correo para activarla.");
                return response;
            }

            if (!user.IsActive)
            {
                response.HasError = true;
                response.Errors.Add($"La cuenta {loginDto.UserName} está inactiva. Contacte con un administrador para activarla.");
                return response;
            }

            var result = await signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                response.HasError = true;

                if (result.IsLockedOut)
                {
                    response.Errors.Add($"La cuenta {loginDto.UserName} ha sido bloqueada por múltiples intentos fallidos. Intenta de nuevo en 10 minutos o usa 'Olvidé mi contraseña'.");
                }
                else
                {
                    response.Errors.Add($"Credenciales inválidas para el usuario {user.UserName}.");
                }

                return response;
            }

            var rolesList = await userManager.GetRolesAsync(user);

            response.Id = user.Id;
            response.Name = user.Name;
            response.LastName = user.LastName;
            response.UserName = user.UserName ?? "";
            response.Email = user.Email ?? "";
            response.DocumentNumber = user.DocumentNumber;
            response.IsVerified = user.EmailConfirmed;
            response.Roles = rolesList.ToList();

            return response;
        }
        public async Task SignOutAsync()
        {
            await signInManager.SignOutAsync();
        }



    }
}
