using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ISavingsAccountRepository : IGenericRepository<SavingsAccount>
    {
        Task<bool> ExistsAccountNumberAsync(string accountNumber);
        Task<SavingsAccount?> GetPrimaryByUserIdAsync(Guid userId);
        Task<List<SavingsAccount>> GetByUserIdAsync(Guid userId);

        // nuevos de Yohansel Mieses - Cuenta de ahorro 

        Task<List<SavingsAccount>> GetPagedAsync(int page, int pageSize);
        Task<List<SavingsAccount>> GetFilteredAsync(bool? isActive, bool? isPrimary, int page, int pageSize);
        Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber);
        Task<SavingsAccount?> GetSecondaryByIdAsync(Guid accountId);
        Task<List<SavingsAccount>> GetAllByUserIdOrderedAsync(Guid userId);
        Task<List<SavingsAccount>> GetAllActiveAsync();
        Task<List<SavingsAccount>> GetActiveAccountsByUserIdAsync(Guid userId);
        Task<List<SavingsAccount>> GetAllByFiltersAsync(bool? isActive, bool? isPrimary);
        Task<SavingsAccount?> GetByIdAsync(Guid accountId);

        //Metod de la funcionalidad de cliente de tranferencia entre cuentas propias

        Task<SavingsAccount?> GetActiveByIdAndUserAsync(Guid accountId, Guid userId);



        Task<List<SavingsAccount>> GetActiveByUserIdAsync(Guid userId);



         Task<bool> UpdateBalanceAsync(Guid accountId, decimal newBalance);

    }
}
