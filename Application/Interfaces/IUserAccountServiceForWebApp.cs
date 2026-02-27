using Application.Dtos.User;
namespace Application.Interfaces
{
    public interface IUserAccountServiceForWebApp : IBaseUserAccountService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task SignOutAsync();
    }
}
