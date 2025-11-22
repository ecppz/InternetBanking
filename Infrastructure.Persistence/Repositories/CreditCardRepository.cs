using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class CreditCardRepository : GenericRepository<CreditCard>, ICreditCardRepository
    {
        public CreditCardRepository(InternetBankingContextDB context) : base(context) { }
        public async Task<bool> HasActiveCardAsync(Guid userId)
        {
            return await context.CreditCards
                .AnyAsync(c => c.UserId == userId && c.Status == CreditCardStatus.Active);
        }

        public async Task<List<CreditCard>> GetActiveCardsAsync()
        {
            return await context.CreditCards
                .Where(c => c.Status == CreditCardStatus.Active)
                .ToListAsync();
        }

        public async Task<List<CreditCard>> GetActiveCardsByUserIdAsync(Guid userId)
        {
            return await context.CreditCards
                .Where(c => c.UserId == userId && c.Status == CreditCardStatus.Active)
                .ToListAsync();
        }

        public async Task<List<CreditCard>> GetCancelledCardsAsync()
        {
            return await context.CreditCards
                .Where(c => c.Status == CreditCardStatus.Cancelled)
                .ToListAsync();
        }

        public async Task<bool> CancelCardAsync(Guid cardId)
        {
            var card = await context.CreditCards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card == null)
            {
                return false;
            }

            if (card.CurrentDebt > 0)
            {
                return false;
            }

            card.Status = CreditCardStatus.Cancelled;
            context.CreditCards.Update(card);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ExpireCardsAsync()
        {
            var now = DateTime.UtcNow;
            var cardsToExpire = await context.CreditCards
                .Where(c => c.Status == CreditCardStatus.Active && c.ExpirationDate < now)
                .ToListAsync();

            foreach (var card in cardsToExpire)
            {
                card.Status = CreditCardStatus.Expired;
            }

            context.CreditCards.UpdateRange(cardsToExpire);
            return await context.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalDebtByUserAsync(Guid userId)
        {
            return await context.CreditCards
                .Where(c => c.UserId == userId && c.Status == CreditCardStatus.Active)
                .SumAsync(c => c.CurrentDebt);
        }

        public async Task<decimal> GetTotalDebtAsync()
        {
            return await context.CreditCards
                .Where(c => c.Status == CreditCardStatus.Active)
                .SumAsync(c => c.CurrentDebt);
        }


        public async Task<int> GetCardCountAsync()
        {
            return await context.CreditCards.CountAsync();
        }

        public async Task<CreditCard?> GetByNumberAsync(string cardNumber)
        {
            return await context.CreditCards
                .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
        }
        public async Task<int> GetActiveCreditCardsCountAsync()
        {
            return await context.CreditCards
                .CountAsync(c => c.Status == CreditCardStatus.Active);
        }
    }
}