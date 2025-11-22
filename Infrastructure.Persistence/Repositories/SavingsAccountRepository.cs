using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

public class SavingsAccountRepository : GenericRepository<SavingsAccount>, ISavingsAccountRepository
{
    public SavingsAccountRepository(InternetBankingContextDB context) : base(context) { }

    public async Task<bool> ExistsAccountNumberAsync(string accountNumber)
    {
        return await context.SavingsAccounts.AnyAsync(a => a.AccountNumber == accountNumber);
    }

    //Para cuenta de ahorro

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

    // Metodos de Yohansel Para cuenta de ahorro

    // Retorna una lista paginada de cuentas de ahorro, ordenadas de la más reciente a la más antigua
    public async Task<List<SavingsAccount>> GetPagedAsync(int page, int pageSize)
    {
        return await context.SavingsAccounts
            .OrderByDescending(sa => sa.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // Retorna una lista paginada de cuentas filtradas por cédula, estado y tipo
    // Retorna una lista paginada de cuentas filtradas por estado y tipo, sin validar cédula
    public async Task<List<SavingsAccount>> GetFilteredAsync(bool? isActive, bool? isPrimary, int page, int pageSize)
    {
        var query = context.SavingsAccounts.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(sa => isActive.Value ? sa.Balance > 0 : sa.Balance == 0);
        }

        if (isPrimary.HasValue)
        {
            query = query.Where(sa => sa.IsPrimary == isPrimary.Value);
        }

        return await query
            .OrderByDescending(sa => sa.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // Busca una cuenta de ahorro por su número identificador único
    public async Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber)
    {
        return await context.SavingsAccounts
            .FirstOrDefaultAsync(sa => sa.AccountNumber == accountNumber);
    }


    public async Task<List<SavingsAccount>> GetActiveAccountsByUserIdAsync(Guid userId)
    {
        return await context.SavingsAccounts
            .Where(a => a.UserId == userId && a.Status == SavingsAccountStatus.Activa)
            .ToListAsync();
    }


    // Retorna una cuenta secundaria por su Id, validando que no sea principal
    public async Task<SavingsAccount?> GetSecondaryByIdAsync(Guid accountId)
    {
        return await context.SavingsAccounts
            .FirstOrDefaultAsync(sa => sa.Id == accountId && !sa.IsPrimary);
    }

    // Retorna todas las cuentas de un usuario, ordenadas de la más reciente a la más antigua
    public async Task<List<SavingsAccount>> GetAllByUserIdOrderedAsync(Guid userId)
    {
        return await context.SavingsAccounts
            .Where(sa => sa.UserId == userId)
            .OrderByDescending(sa => sa.Id)
            .ToListAsync();
    }

    // Retorna todas las cuentas activas del sistema (con balance mayor a cero)
    public async Task<List<SavingsAccount>> GetAllActiveAsync()
    {
        return await context.SavingsAccounts
            .Where(sa => sa.Balance > 0)
            .OrderByDescending(sa => sa.Id)
            .ToListAsync();
    }

    // Retorna todas las cuentas filtradas por estado y tipo, sin validar cédula
    public async Task<List<SavingsAccount>> GetAllByFiltersAsync(bool? isActive, bool? isPrimary)
    {
        var query = context.SavingsAccounts.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(sa => isActive.Value ? sa.Balance > 0 : sa.Balance == 0);
        }

        if (isPrimary.HasValue)
        {
            query = query.Where(sa => sa.IsPrimary == isPrimary.Value);
        }

        return await query
            .OrderByDescending(sa => sa.Id)
            .ToListAsync();
    }

    public async Task<SavingsAccount?> GetByIdAsync(Guid accountId)
    {
        return await context.SavingsAccounts
            .FirstOrDefaultAsync(sa => sa.Id == accountId);
    }


    //Para cuenta de ahorro aqui finalizan sus metodos


    //Metod de la funcionalidad de cliente de tranferencia entre cuentas propias


    public async Task<SavingsAccount?> GetActiveByIdAndUserAsync(Guid accountId, Guid userId)
    {
        return await context.SavingsAccounts
            .FirstOrDefaultAsync(sa =>
                sa.Id == accountId &&
                sa.UserId == userId &&
                sa.Status == SavingsAccountStatus.Activa);
    }

    public async Task<List<SavingsAccount>> GetActiveByUserIdAsync(Guid userId)
    {
        return await context.SavingsAccounts
            .Where(a => a.UserId == userId && a.Status == SavingsAccountStatus.Activa)
            .OrderByDescending(a => a.AccountNumber)
            .ToListAsync();
    }

    public async Task<bool> UpdateBalanceAsync(Guid accountId, decimal newBalance)
    {
        var account = await context.SavingsAccounts.FindAsync(accountId);
        if (account == null)
            return false;

        account.Balance = newBalance;

        context.SavingsAccounts.Update(account);
        var changes = await context.SaveChangesAsync();

        return changes > 0;
    }
    //para admin

    // Retorna todas las cuentas de ahorro registradas en el sistema.
    // Se ordenan por fecha de creación (Id como referencia) descendente.
    public async Task<List<SavingsAccount>> GetAllSavingsAccountsAsync()
    {
        return await context.SavingsAccounts
            .OrderByDescending(sa => sa.Id) // si tienes campo DateCreated, cámbialo aquí
            .ToListAsync();
    }

    public async Task<bool> SetAccountStatusAsync(Guid accountId, SavingsAccountStatus newStatus)
    {
        var account = await context.SavingsAccounts.FirstOrDefaultAsync(a => a.Id == accountId);
        if (account == null) return false;

        account.Status = newStatus;
        await context.SaveChangesAsync();
        return true;
    }


    public async Task<bool> AddBalanceToPrimaryAccountAsync(Guid userId, decimal amount)
    {
        // Buscar la cuenta principal del usuario
        var account = await context.SavingsAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsPrimary);

        if (account == null)
            return false;

        // Sumar el monto adicional al balance existente
        account.Balance += amount;

        // Guardar cambios
        context.SavingsAccounts.Update(account);
        await context.SaveChangesAsync();

        return true;
    }


}
