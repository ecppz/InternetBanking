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
        public async Task<List<Transaction>> GetByStatusAsync(string status)
        {
            // Parse el string a enum
            if (!Enum.TryParse<TransactionStatus>(status, true, out var parsedStatus))
            {
                // Si no se puede convertir, retorna lista vacía
                return new List<Transaction>();
            }

            return await context.Transactions
                .Where(t => t.Status == parsedStatus)
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

        // Retorna todas las transacciones registradas en el sistema.
        // Se ordenan por fecha descendente para que las más recientes aparezcan primero.
        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return await context.Transactions
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }
        //home  cajero ------------------------------------------------------------------------------------------------------------

       //transacciones
        public async Task<int> GetTransactionsByCashierAndDateAsync(Guid cashierId, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            return await context.Transactions
                .CountAsync(t => t.PerformedByUserId == cashierId &&
                            t.Date >= start && t.Date < end);
        }
        //pagos

        public async Task<int> GetPaymentsCountByCashierAndDateAsync(Guid cashierId, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            return await context.Transactions
                .CountAsync(t => t.PerformedByUserId == cashierId &&        
                                 (t.Type == TransactionType.LoanPayment ||  
                                  t.Type == TransactionType.CreditCardPayment)  
                                 && t.Date >= start && t.Date < end);
        }
        //depositos
        public async Task<int> GetDepositsCountByCashierAndDateAsync(Guid cashierId, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            return await context.Transactions
                .CountAsync(t => t.PerformedByUserId == cashierId &&  
                                 t.Type == TransactionType.Deposit &&
                                 t.Date >= start && t.Date < end);
        }
        //retiros
        public async Task<int> GetWithdrawalsCountByCashierAndDateAsync(Guid cashierId, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            return await context.Transactions
                .CountAsync(t => t.PerformedByUserId == cashierId &&  
                                 t.Type == TransactionType.CashWithdrawal &&
                                 t.Date >= start && t.Date < end);
        }

        //------
        public async Task<(int TotalPayments, int TodayPayments)> GetLoanAndCreditCardPaymentsAsync()
        {
           var approvedPayments = await context.Transactions
               .Where(t => (t.Type == TransactionType.LoanPayment ||
                            t.Type == TransactionType.CreditCardPayment) &&
                            t.Status == TransactionStatus.Approved)
               .ToListAsync();

           var totalPayments = approvedPayments.Count;
           var todayPayments = approvedPayments.Count(t => t.Date.Date == DateTime.Today);

             return (totalPayments, todayPayments);
        }
    }
}