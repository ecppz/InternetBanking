using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(InternetBankingContextDB context) : base(context) { }

        //Para cuenta de ahorro

        // Retorna todas las transacciones donde la cuenta fue origen del movimiento
        public async Task<List<Transaction>> GetByOriginAccountIdAsync(Guid accountId)
        {
            return await context.Transactions
                .Where(t => t.OriginAccountId == accountId)
                .ToListAsync();
        }

        // Retorna todas las transacciones donde la cuenta fue destino del movimiento
        public async Task<List<Transaction>> GetByDestinationAccountIdAsync(Guid accountId)
        {
            return await context.Transactions
                .Where(t => t.DestinationAccountId == accountId)
                .ToListAsync();
        }

        // Retorna todas las transacciones donde la cuenta fue origen o destino
        public async Task<List<Transaction>> GetAllByAccountIdAsync(Guid accountId)
        {
            return await context.Transactions
                .Where(t => t.OriginAccountId == accountId || t.DestinationAccountId == accountId)
                .ToListAsync();
        }

        // Retorna todas las transacciones donde la cuenta fue origen o destino, ordenadas de más reciente a más antigua
        public async Task<List<Transaction>> GetAllByAccountIdOrderedAsync(Guid accountId)
        {
            return await context.Transactions
                .Where(t => t.OriginAccountId == accountId || t.DestinationAccountId == accountId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        //para cajero

        public async Task<bool> RegisterTransactionAsync(Transaction transaction)
        {
            await context.Transactions.AddAsync(transaction);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<List<Transaction>> GetByTypeAsync(TransactionType type)
        {
            return await context.Transactions
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }
        public async Task<List<Transaction>> GetByStatusAsync(TransactionStatus status)
        {
            return await context.Transactions
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }


        //Aqui finaliza los metodos de cuenta de ahoo   

        public async Task<bool> ExecuteInternalTransferAsync(
            SavingsAccount originAccount,
            SavingsAccount destinationAccount,
            decimal amount,
            Transaction debitTransaction,
            Transaction creditTransaction
        )
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Actualizar balances
                originAccount.Balance -= amount;
                destinationAccount.Balance += amount;

                context.SavingsAccounts.Update(originAccount);
                context.SavingsAccounts.Update(destinationAccount);

                // Registrar transacciones
                await context.Transactions.AddAsync(debitTransaction);
                await context.Transactions.AddAsync(creditTransaction);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }


       

    }
}