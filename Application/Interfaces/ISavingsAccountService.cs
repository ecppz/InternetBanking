using Application.Dtos.SavingsAccount;

namespace Application.Interfaces
{
    public interface ISavingsAccountService : IGenericService<SavingsAccountDto>
    {
        Task<string> GenerateUniqueAccountNumberAsync();
        Task<bool> AddBalanceAsync(Guid accountId, decimal amount);
        Task<bool> ExistsAccountNumberAsync(string accountNumber);
        Task<SavingsAccountDetailDto?> GetAccountDetailAsync(Guid accountId);
        Task<List<SavingsAccountSummaryDto>> GetFilteredAccountsAsync(string? documentNumber, bool? isActive, bool? isPrimary, int page, int pageSize);
        Task<List<SavingsAccountDto>> GetAllByUserIdOrderedAsync(Guid userId);
        Task<bool> CreateSecondaryAccountAsync(CreateSavingsAccountDto dto);
        Task<bool> CancelSecondaryAccountAsync(Guid accountId);
        Task<SavingsAccountSummaryDto?> GetAccountSummaryAsync(Guid accountId);
        Task<Guid?> GetAccountIdByAccountNumberAsync(string accountNumber);
        Task<List<SavingsAccountDto>> GetActiveAccountsByUserIdAsync(Guid userId);

        Task<List<SavingsAccountSummaryDto>> GetActiveAccountSummariesByUserIdAsync(Guid userId);
        //metodo para obtener las cuentas activas del cliente:

        Task<List<SavingsAccountDto>> GetActiveByUserIdAsync(Guid userId);

        Task<SavingsAccountDto?> GetByAccountNumberAsync(string accountNumber);

        // para el home de administrdor

        Task<List<SavingsAccountDto>> GetAllSavingsAccountsAsync();


        Task<bool> ActivatePrimaryAccountAsync(Guid userId);


        Task<bool> DeactivatePrimaryAccountAsync(Guid userId);


        Task<bool> AddBalanceToPrimaryAccountAsync(Guid userId, decimal amount);



    }
}
