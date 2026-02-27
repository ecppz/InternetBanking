using Application.Dtos.User;
namespace Application.Interfaces
{
    public interface IBaseUserAccountService
    {
        Task<UserResponseDto> ConfirmAccountAsync(string userId, string token);
        Task<EditResponseDto> EditUser(SaveUserDto saveDto, string? origin, bool? isCreated = false, bool? isApi = false);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request,  bool? isApi = false);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request, bool? isApi = false);
        Task<List<UserDto>> GetAllActiveUsers();
        Task<UserDto?> GetUserByEmail(string email);
        Task<UserDto?> GetUserByUserName(string userName);
        Task<UserDto?> GetUserById(string Id);
        Task<List<UserDto>> GetUsersByIds(List<Guid> ids);
        Task<IList<string>> GetUserRolesAsync(Guid userId);
        Task<SaveUserResponseDto> SaveUser(SaveUserDto saveDto, string origin);
        Task<List<Guid>> GetAllUserIdsAsync();
        Task<RegisterResponseDto> RegisterUserAsync(SaveUserDto dto, string? origin, bool? isApi = false);
        Task<bool> SetUserActiveStatus(string id, bool isActive, bool? isApi = false);
        Task<List<UserDto>> GetAllCustomersAsync();
    }
}
