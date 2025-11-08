using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ISavingsAccountRepository : IGenericRepository<SavingsAccount>
    {
        Task<bool> ExistsAccountNumberAsync(string accountNumber);
        Task<SavingsAccount?> GetPrimaryByUserIdAsync(Guid userId);
        Task<List<SavingsAccount>> GetByUserIdAsync(Guid userId);
    }
}
