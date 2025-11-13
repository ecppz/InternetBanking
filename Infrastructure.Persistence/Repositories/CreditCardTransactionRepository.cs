using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;

namespace Infrastructure.Persistence.Repositories
{
    public class CreditCardTransactionRepository : GenericRepository<CreditCardTransaction>, ICreditCardTransactionRepository
    {
        public CreditCardTransactionRepository(InternetBankingContextDB context) : base(context) { }

    }
}