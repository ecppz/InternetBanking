using Application.Dtos.User;

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
        Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string origin);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task SignOutAsync();
        Task<List<Guid>> GetAllUserIdsAsync();
    }
}
