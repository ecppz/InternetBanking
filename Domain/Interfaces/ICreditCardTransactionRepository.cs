using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICreditCardTransactionRepository : IGenericRepository<CreditCardTransaction>
    {
        Task<List<CreditCardTransaction>> GetAllTransactionsAsync();
        Task<List<CreditCardTransaction>> GetByCardIdAsync(Guid cardId);


    }
}
