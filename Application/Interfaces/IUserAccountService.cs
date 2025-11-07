using Application.Dtos.User;
using static Application.Dtos.User.UserResponseDto;

namespace Application.Interfaces
{
    public interface IUserAccountService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task<EditResponseDto> EditUser(SaveUserDto saveDto, string origin, bool? isCreated = false);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<List<UserDto>> GetAllUser(bool? isActive = true);
        Task<UserDto?> GetUserByEmail(string email);
        Task<UserDto?> GetUserByUserName(string userName);
        Task<UserDto?> GetUserById(string Id);
        Task<List<UserDto>> GetUsersByIds(List<Guid> ids);
        Task<SaveUserResponseDto> SaveUser(SaveUserDto saveDto, string origin);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task SignOutAsync();
        Task<List<Guid>> GetAllUserIdsAsync();
        Task<bool> ActivateUser(string id);
        Task<bool> DeactivateUser(string id);

        //Nueva firma broki

        Task<UserResponseDto> RegisterUserAsync(SaveUserDto dto, string origin);

        //unico metodo para activar y desactivar:

        Task<bool> SetUserActiveStatus(string id, bool isActive);

    }
}
