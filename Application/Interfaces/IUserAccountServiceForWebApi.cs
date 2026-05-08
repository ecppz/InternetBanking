using Application.Dtos.User;
namespace Application.Interfaces
{
    public interface IUserAccountServiceForWebApi : IBaseUserAccountService
    {
        Task<LoginResponseForApiDto> AuthenticateAsync(LoginDto loginDto);
        Task<bool> ExistsByCommerceIdAsync(int commerceId);
    }
}
