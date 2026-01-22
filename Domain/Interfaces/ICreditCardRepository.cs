using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICreditCardRepository : IGenericRepository<CreditCard>
    {
        Task<bool> HasActiveCardAsync(Guid userId);
        Task<List<CreditCard>> GetActiveCardsAsync();
        Task<List<CreditCard>> GetActiveCardsByUserIdAsync(Guid userId);
        Task<List<CreditCard>> GetCancelledCardsAsync();
        Task<bool> CancelCardAsync(Guid cardId);
        Task<int> ExpireCardsAsync();
        Task<decimal> GetTotalDebtByUserAsync(Guid userId);
        Task<decimal> GetTotalDebtAsync();
        Task<int> GetCardCountAsync();
        Task<CreditCard?> GetByNumberAsync(string cardNumber);
        Task<int> GetActiveCreditCardsCountAsync();

    }
}
