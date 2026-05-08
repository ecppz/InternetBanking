using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
namespace Infrastructure.Persistence.Repositories
{
    public class CommerceRepository : GenericRepository<Commerce>, ICommerceRepository
    {
        public CommerceRepository(InternetBankingContextDB context) : base(context) { }
    }
}