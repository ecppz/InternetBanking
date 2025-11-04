using Application.Dtos.Email;
using Application.Dtos.User;
using Application.Interfaces;
using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Infrastructure.Identity.Services
{
    public class UserAccountService : IUserAccountService
    {
        private readonly UserManager<UserAccount> userManager;
        private readonly SignInManager<UserAccount> signInManager;
        private readonly IEmailService emailService;
        public UserAccountService(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, IEmailService emailService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            LoginResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var user = await userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"no hay cuenta registradas con este username: {loginDto.UserName}");
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Errors.Add($"esta cuenta {loginDto.UserName} no esta activa deberias chequear tu email");
                return response;
            }

            var result = await signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                response.HasError = true;

                if (result.IsLockedOut)
                {
                    response.Errors.Add($"tu cuenta {loginDto.UserName} ha sido bloqueada debido a múltiples intentos fallidos." +
                        $"inténtalo de nuevo en 10 minutos. Si no recuerdas tu contraseña, puedes darle a 'olvidé mi contraseña'");
                }
                else
                {
                    response.Errors.Add($"Estas credenciales no son válidas para el usuario {user.UserName}.");
                }

                return response;
            }


            var rolesList = await userManager.GetRolesAsync(user);

            response.Id = user.Id;
            response.Email = user.Email ?? "";
            response.UserName = user.UserName ?? "";
            response.Name = user.Name;
            response.LastName = user.LastName;
            response.IsVerified = user.EmailConfirmed;
            response.Roles = rolesList.ToList();

            return response;
        }
        public async Task SignOutAsync()
        {
            await signInManager.SignOutAsync();
        }
        public async Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string origin)
        {
            RegisterResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await userManager.FindByNameAsync(saveDto.UserName);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"este username: {saveDto.UserName} ya está en uso");
                return response;
            }

            var userWithSameEmail = await userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"este email: {saveDto.Email} ya está en uso");
                return response;
            }

            UserAccount user = new UserAccount()
            {
                Name = saveDto.Name,
                LastName = saveDto.LastName,
                Email = saveDto.Email,
                UserName = saveDto.UserName,
                ProfileImage = saveDto.ProfileImage,
                EmailConfirmed = false,
                PhoneNumber = saveDto.Phone
            };

            var result = await userManager.CreateAsync(user, saveDto.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Roles.User.ToString());
                string verificationUri = await GetVerificationEmailUri(user, origin);
                await emailService.SendAsync(new EmailRequestDto()
                {
                    To = saveDto.Email,
                    HtmlBody = $"Porfavor confirma tu cuenta visitando esta URL <a href='{verificationUri}'> Clic aqui </a>",
                    Subject = "Confirmar registro"
                });

                var rolesList = await userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.IsVerified = user.EmailConfirmed;
                response.Roles = rolesList.ToList();

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }
        public async Task<EditResponseDto> EditUser(SaveUserDto saveDto, string origin, bool? isCreated = false)
        {
            bool isNotcreated = !isCreated ?? false;
            EditResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await userManager.Users.FirstOrDefaultAsync(w => w.UserName == saveDto.UserName && w.Id != saveDto.Id);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"este username: {saveDto.UserName} ya está en uso");
                return response;
            }

            var userWithSameEmail = await userManager.Users.FirstOrDefaultAsync(w => w.Email == saveDto.Email && w.Id != saveDto.Id);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"este email: {saveDto.Email} ya está en uso");
                return response;
            }

            var user = await userManager.FindByIdAsync(saveDto.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"esta cuenta no esta registrada con este usuario");
                return response;
            }

            user.Name = saveDto.Name;
            user.LastName = saveDto.LastName;
            user.UserName = saveDto.UserName;
            user.ProfileImage = string.IsNullOrWhiteSpace(saveDto.ProfileImage) ? user.ProfileImage : saveDto.ProfileImage;
            user.EmailConfirmed = user.EmailConfirmed && user.Email == saveDto.Email;
            user.Email = saveDto.Email;
            user.PhoneNumber = saveDto.Phone;

            if (!string.IsNullOrWhiteSpace(saveDto.Password) && isNotcreated)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var resultChange = await userManager.ResetPasswordAsync(user, token, saveDto.Password);

                if (resultChange != null && !resultChange.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(resultChange.Errors.Select(s => s.Description).ToList());
                    return response;
                }
            }

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var rolesList = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, rolesList.ToList());
                await userManager.AddToRoleAsync(user, Roles.User.ToString());

                if (!user.EmailConfirmed && isNotcreated)
                {
                    string verificationUri = await GetVerificationEmailUri(user, origin);
                    await emailService.SendAsync(new EmailRequestDto()
                    {
                        To = saveDto.Email,
                        HtmlBody = $"Porfavor confirma tu cuenta visitando esta URL <a href='{verificationUri}'> Clic aqui </a>",
                        Subject = "Confirmar registro"
                    });
                }

                var updatedRolesList = await userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.IsVerified = user.EmailConfirmed;
                response.Roles = updatedRolesList.ToList();

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }

        public async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await userManager.FindByNameAsync(request.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"no hay cuenta registrada con este usuario{request.UserName}");
                return response;
            }

            var resetUri = await GetResetPasswordUri(user, request.Origin);
            user.EmailConfirmed = false;

            await userManager.UpdateAsync(user);

            await emailService.SendAsync(new EmailRequestDto()
            {
                To = user.Email,
                HtmlBody = $"por favor resetea tu password account visitando este URL <a href='{resetUri}'> Clic aqui </a>",
                Subject = "Reset password"
            });

            return response;
        }

        public async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"no hay cuenta registrada con este usuario");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);

            return response;
        }
        public async Task<UserDto?> GetUserByEmail(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            var rolesList = await userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                ProfileImage = user.ProfileImage,
                Phone = user.PhoneNumber,
                isVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? ""
            };

            return userDto;
        }
        public async Task<UserDto?> GetUserById(string Id)
        {
            var user = await userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return null;
            }

            var rolesList = await userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                ProfileImage = user.ProfileImage,
                Phone = user.PhoneNumber,
                isVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? ""
            };

            return userDto;
        }
        public async Task<List<UserDto>> GetUsersByIds(List<Guid> ids)
        {
            var users = await userManager.Users
                .Where(u => ids.Select(id => id.ToString()).Contains(u.Id))
                .ToListAsync();

            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User"; 

                result.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    Name = user.Name ?? string.Empty,
                    LastName = user.LastName,
                    Phone = user.PhoneNumber,
                    ProfileImage = user.ProfileImage,
                    isVerified = user.EmailConfirmed,
                    Role = role
                });
            }

            return result;
        }


        public async Task<UserDto?> GetUserByUserName(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return null;
            }

            var rolesList = await userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                ProfileImage = user.ProfileImage,
                Phone = user.PhoneNumber,
                isVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? ""
            };

            return userDto;
        }
        public async Task<List<UserDto>> GetAllUser(bool? isActive = true)
        {
            List<UserDto> listUsersDtos = [];

            var users = userManager.Users;

            if (isActive != null && isActive == true)
            {
                users = users.Where(w => w.EmailConfirmed);
            }

            var listUser = await users.ToListAsync();

            foreach (var item in listUser)
            {
                var roleList = await userManager.GetRolesAsync(item);

                listUsersDtos.Add(new UserDto()
                {
                    Id = item.Id,
                    Email = item.Email ?? "",
                    LastName = item.LastName,
                    Name = item.Name,
                    UserName = item.UserName ?? "",
                    ProfileImage = item.ProfileImage,
                    Phone = item.PhoneNumber,
                    isVerified = item.EmailConfirmed,
                    Role = roleList.FirstOrDefault() ?? ""
                });
            }

            return listUsersDtos;
        }

        public async Task<List<Guid>> GetAllUserIdsAsync()
        {
            return await userManager.Users
                   .Select(u => Guid.Parse(u.Id))
                   .ToListAsync();
        }


        public async Task<string> ConfirmAccountAsync(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "no existe una cuenta registrada con este usuario.";
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return $"cuenta confirmada para {user.Email}. ya puedes usar la app";
            }
            else
            {
                return $"ocurrió un error al confirmar el correo electrónico {user.Email}.";
            }
        }

        //Private methods

        private async Task<string> GetVerificationEmailUri(UserAccount user, string origin)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ConfirmEmail";
            var completeUrl = new Uri(string.Concat(origin, "/", route));// origin = https://localhost:58296 route=Login/ConfirmEmail
            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri.ToString(), "token", token);

            return verificationUri;
        }
        private async Task<string> GetResetPasswordUri(UserAccount user, string origin)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route));// origin = https://localhost:58296 route=Login/ConfirmEmail
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri.ToString(), "token", token);

            return resetUri;
        }

    }
}
