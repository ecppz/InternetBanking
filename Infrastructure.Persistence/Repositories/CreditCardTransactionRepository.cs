using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class CreditCardTransactionRepository : GenericRepository<CreditCardTransaction>, ICreditCardTransactionRepository
    {
        public CreditCardTransactionRepository(InternetBankingContextDB context) : base(context) { }

        public async Task<List<CreditCardTransaction>> GetAllTransactionsAsync()
        {
            return await context.CreditCardTransactions.ToListAsync();
        }
        public async Task<List<CreditCardTransaction>> GetByCardIdAsync(Guid cardId)
        {
            return await context.CreditCardTransactions
                .Where(t => t.CreditCardId == cardId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

    }
}