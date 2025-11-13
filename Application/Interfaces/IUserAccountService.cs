using Application.Dtos.User;
namespace Application.Interfaces
{
    public interface IUserAccountService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task<EditResponseDto> EditUser(SaveUserDto saveDto, string origin, bool? isCreated = false);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<List<UserDto>> GetAllActiveUsers();
        Task<UserDto?> GetUserByEmail(string email);
        Task<UserDto?> GetUserByUserName(string userName);
        Task<UserDto?> GetUserById(string Id);
        Task<List<UserDto>> GetUsersByIds(List<Guid> ids);
        Task<IList<string>> GetUserRolesAsync(Guid userId);
        Task<SaveUserResponseDto> SaveUser(SaveUserDto saveDto, string origin);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task SignOutAsync();
        Task<List<Guid>> GetAllUserIdsAsync();
        Task<UserResponseDto> RegisterUserAsync(SaveUserDto dto, string origin);
        Task<bool> SetUserActiveStatus(string id, bool isActive);

    }
}
