using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;

public class SavingsAccountRepository : GenericRepository<SavingsAccount>, ISavingsAccountRepository
{
    public SavingsAccountRepository(InternetBankingContextDB context) : base(context) {}

    public async Task<bool> ExistsAccountNumberAsync(string accountNumber)
    {
        return await context.SavingsAccounts.AnyAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<List<SavingsAccount>> GetByUserIdAsync(Guid userId)
    {
        return await context.SavingsAccounts
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public async Task<SavingsAccount?> GetPrimaryByUserIdAsync(Guid userId)
    {
        return await context.SavingsAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsPrimary);
    }
}
