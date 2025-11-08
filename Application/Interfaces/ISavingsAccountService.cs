using Application.Dtos.SavingsAccount;

namespace Application.Interfaces
{
    public interface ISavingsAccountService : IGenericService<SavingsAccountDto>
    {
        Task<string> GenerateUniqueAccountNumberAsync();
        Task<bool> AddBalanceAsync(Guid accountId, decimal amount);
        Task<bool> ExistsAccountNumberAsync(string accountNumber);
    }
}
