
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Infrastructure.Persistence.Repositories
{
    public class CreditCardTransactionRepository : GenericRepository<CreditCardTransaction>, ICreditCardTransactionRepository
    {
        public CreditCardTransactionRepository(InternetBankingContextDB context) : base(context) { }














        public async Task<List<CreditCardTransaction>> GetAllTransactionsAsync()
        {
            return await context.CreditCardTransactions.ToListAsync();
        }

    }
}