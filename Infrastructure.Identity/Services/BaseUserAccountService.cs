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
    public abstract class BaseUserAccountService : IBaseUserAccountService
    {
        private readonly UserManager<UserAccount> userManager;
        private readonly IEmailService emailService;
        public BaseUserAccountService(UserManager<UserAccount> userManager, IEmailService emailService)
        {
            this.userManager = userManager;
            this.emailService = emailService;
        }
        public virtual async Task<SaveUserResponseDto> SaveUser(SaveUserDto saveDto, string origin)
        {
            SaveUserResponseDto response = new()
            {
                Id = "",
                Name = "",
                LastName = "",
                UserName = "",
                Email = "",
                DocumentNumber = "",
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
                UserName = saveDto.UserName,
                Email = saveDto.Email,
                DocumentNumber = saveDto.DocumentNumber,
                EmailConfirmed = false,
                IsActive = false
            };

            var result = await userManager.CreateAsync(user, saveDto.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Roles.Customer.ToString());
                string verificationUri = await GetVerificationEmailUri(user, origin);
                await emailService.SendAsync(new EmailRequestDto()
                {
                    To = saveDto.Email,
                    HtmlBody = $"Porfavor confirma tu cuenta visitando esta URL <a href='{verificationUri}'> Clic aqui </a>",
                    Subject = "Confirmar registro"
                });

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
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }
        public virtual async Task<EditResponseDto> EditUser(SaveUserDto saveDto, string? origin, bool? isCreated = false, bool? isApi = false)
        {
            bool isNotcreated = !isCreated ?? false;
            EditResponseDto response = new()
            {
                Id = "",
                Name = "",
                LastName = "",
                UserName = "",
                Email = "",
                DocumentNumber = "",
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
            user.EmailConfirmed = user.EmailConfirmed && user.Email == saveDto.Email;
            user.Email = saveDto.Email;
            user.DocumentNumber = saveDto.DocumentNumber;

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
                var currentRoles = await userManager.GetRolesAsync(user);
                if (!string.IsNullOrWhiteSpace(saveDto.Role) && !currentRoles.Contains(saveDto.Role))
                {
                    await userManager.RemoveFromRolesAsync(user, currentRoles);
                    await userManager.AddToRoleAsync(user, saveDto.Role);
                }

                if (!user.EmailConfirmed && isNotcreated)
                {
                    string verificationUri = await GetVerificationEmailUri(user, origin);
                    await emailService.SendAsync(new EmailRequestDto()
                    {
                        To = saveDto.Email,
                        HtmlBody = $"Por favor confirma tu cuenta visitando esta URL <a href='{verificationUri}'> Clic aqui </a>",
                        Subject = "Confirmar registro"
                    });
                }

                var updatedRolesList = await userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.UserName = user.UserName ?? "";
                response.Email = user.Email ?? "";
                response.DocumentNumber = user.DocumentNumber;
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

        public virtual async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false)
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

        public virtual async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request, bool? isApi = false)
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
        public virtual async Task<UserDto?> GetUserByEmail(string email)
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
                Name = user.Name,
                LastName = user.LastName,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                DocumentNumber = user.DocumentNumber,
                isVerified = user.EmailConfirmed,
                IsActive = user.IsActive,
                Role = rolesList.FirstOrDefault() ?? ""
            };

            return userDto;
        }
        public virtual async Task<UserDto?> GetUserById(string Id)
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
                Name = user.Name,
                LastName = user.LastName,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                DocumentNumber = user.DocumentNumber,
                isVerified = user.EmailConfirmed,
                IsActive = user.IsActive,
                Role = rolesList.FirstOrDefault() ?? ""
            };

            return userDto;
        }
        public virtual async Task<List<UserDto>> GetUsersByIds(List<Guid> ids)
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
                    Name = user.Name ?? string.Empty,
                    LastName = user.LastName,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    DocumentNumber = user.DocumentNumber,
                    isVerified = user.EmailConfirmed,
                    IsActive = user.IsActive,
                    Role = role,
                });
            }

            return result;
        }

        public virtual async Task<UserDto?> GetUserByUserName(string userName)
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
                Name = user.Name,
                LastName = user.LastName,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                DocumentNumber = user.DocumentNumber,
                isVerified = user.EmailConfirmed,
                IsActive = user.IsActive,
                Role = rolesList.FirstOrDefault() ?? ""
            };

            return userDto;
        }
        public virtual async Task<List<UserDto>> GetAllActiveUsers()
        {
            var listUsersDtos = new List<UserDto>();
            var users = userManager.Users.Where(u => u.IsActive);

            var listUser = await users.ToListAsync();

            foreach (var item in listUser)
            {
                var roleList = await userManager.GetRolesAsync(item);
                var role = roleList.FirstOrDefault() ?? "";

                if (role == Roles.Commerce.ToString())
                {
                    continue;
                }

                listUsersDtos.Add(new UserDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    LastName = item.LastName,
                    UserName = item.UserName ?? "",
                    Email = item.Email ?? "",
                    DocumentNumber = item.DocumentNumber,
                    isVerified = item.EmailConfirmed,
                    Role = role,
                    IsActive = item.IsActive
                });
            }

            return listUsersDtos;
        }

        public virtual async Task<IList<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            return user == null ? new List<string>() : await userManager.GetRolesAsync(user);
        }

        public virtual async Task<List<Guid>> GetAllUserIdsAsync()
        {
            var users = await userManager.Users.ToListAsync();  
            var filteredIds = new List<Guid>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                if (role == Roles.Commerce.ToString())
                {
                    continue;
                }

                filteredIds.Add(Guid.Parse(user.Id));
            }

            return filteredIds;
        }


        public virtual async Task<UserResponseDto> ConfirmAccountAsync(string userId, string token)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.Message = "There is no acccount registered with this user";
                response.HasError = true;
                return response;
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                response.Message = $"Account confirmed for {user.Email}. You can now use the app";
                response.HasError = false;
                return response;
            }
            else
            {
                response.Message = $"An error occurred while confirming this email {user.Email}";
                response.HasError = true;
                return response;
            }
        }
        public virtual async Task<RegisterResponseDto> RegisterUserAsync(SaveUserDto dto, string? origin, bool? isApi = false)
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
            try
            {
                // Inicializar usuario
                var user = new UserAccount
                {
                    Name = dto.Name,
                    LastName = dto.LastName,
                    DocumentNumber = dto.DocumentNumber,
                    UserName = dto.UserName,
                    Email = dto.Email,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    IsActive = false,
                };

                if (dto.Role == Roles.Admin.ToString() || dto.Role == Roles.Cashier.ToString())
                {
                    user.EmailConfirmed = true;
                }
                else if (dto.Role == Roles.Customer.ToString())
                {
                    user.EmailConfirmed = false;
                }


                var result = await userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(result.Errors.Select(e => e.Description));
                    return response;
                }

                // Asignar rol
                var roleResult = await userManager.AddToRoleAsync(user, dto.Role);
                if (!roleResult.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(roleResult.Errors.Select(e => e.Description));
                    return response;
                }

                // Solo clientes reciben correo de confirmación
                if (dto.Role == Roles.Customer.ToString())
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out var baseUri))
                    {
                        response.HasError = true;
                        response.Errors.Add("El parámetro 'origin' no es una URI válida.");
                        return response;
                    }

                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    var confirmUrl = new Uri(baseUri, "Login/ConfirmEmail");
                    var verificationUri = QueryHelpers.AddQueryString(confirmUrl.ToString(), "userId", user.Id);
                    verificationUri = QueryHelpers.AddQueryString(verificationUri, "token", token);

                    var subject = "Confirma tu cuenta";
                    var body = $"""
                        Hola {user.Name},<br><br>
                        Por favor confirma tu cuenta haciendo clic en el siguiente enlace:<br>
                        <a href='{verificationUri}'>Confirmar cuenta</a>
                     """;

                    await emailService.SendAsync(new EmailRequestDto
                    {
                        To = user.Email,
                        Subject = subject,
                        HtmlBody = body
                    });
                }

                // Respuesta
                response.HasError = false;
                response.Id = user.Id;
                response.Email = user.Email;
                response.UserName = user.UserName;
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.Roles.Add(dto.Role);

                return response;
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Errors.Add("Error interno: " + ex.Message);
                return response;
            }
        }

        public virtual async Task<bool> SetUserActiveStatus(string id, bool isActive, bool? isApi = false)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return false;

            user.IsActive = isActive;
            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public virtual async Task<List<UserDto>> GetAllCustomersAsync()
        {
            // Obtiene todos los usuarios desde el UserManager
            var users = userManager.Users.ToList();

            List<UserDto> customers = new();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                // Filtra solo los que tienen rol "Customer"
                if (roles.Contains(Roles.Customer.ToString()))
                {
                    customers.Add(new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        LastName = user.LastName,
                        UserName = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        DocumentNumber = user.DocumentNumber,
                        IsActive = user.IsActive,
                        Role = Roles.Customer.ToString(),
                        isVerified = user.EmailConfirmed,
                    });
                }
            }

            return customers;
        }


        #region protected methods

        protected async Task<string> GetVerificationEmailUri(UserAccount user, string origin)
        {
            if (string.IsNullOrWhiteSpace(origin) || !Uri.IsWellFormedUriString(origin, UriKind.Absolute))
            {
                throw new InvalidOperationException("El parámetro 'origin' no es una URI válida.");
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var baseUri = new Uri(origin.TrimEnd('/'));
            var completeUrl = new Uri(baseUri, "Login/ConfirmEmail");

            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "token", token);

            return verificationUri;
        }
        protected async Task<string?> GetVerificationEmailToken(UserAccount user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }

        protected async Task<string> GetResetPasswordUri(UserAccount user, string origin)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route));// origin = https://localhost:58296 route=Login/ConfirmEmail
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri.ToString(), "token", token);

            return resetUri;
        }

        protected async Task<string?> GetResetPasswordToken(UserAccount user)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }

        #endregion

    }
}
