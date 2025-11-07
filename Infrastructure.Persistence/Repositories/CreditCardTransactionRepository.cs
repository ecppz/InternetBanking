using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;

namespace Persistence.Repositories
{
    public class CreditCardTransactionRepository : GenericRepository<CreditCardTransaction>, ICreditCardTransactionRepository
    {
        public CreditCardTransactionRepository(InternetBankingContextDB context) : base(context) { }

    }
}