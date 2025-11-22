using Application.Dtos.SavingsAccount;

namespace Application.Interfaces
{
    public interface ISavingsAccountService : IGenericService<SavingsAccountDto>
    {
        Task<string> GenerateUniqueAccountNumberAsync();
        Task<bool> AddBalanceAsync(Guid accountId, decimal amount);
        Task<bool> ExistsAccountNumberAsync(string accountNumber);

        //Gestion de cuentas de ahorro

        // Retorna el detalle completo de una cuenta, incluyendo transacciones y datos del cliente
        Task<SavingsAccountDetailDto?> GetAccountDetailAsync(Guid accountId);

        // Retorna un listado paginado de cuentas con filtros aplicables desde el servicio
        Task<List<SavingsAccountSummaryDto>> GetFilteredAccountsAsync(string? documentNumber, bool? isActive, bool? isPrimary, int page, int pageSize);

        // Retorna todas las cuentas de un cliente ordenadas por fecha
        Task<List<SavingsAccountDto>> GetAllByUserIdOrderedAsync(Guid userId);

        // Crea una cuenta secundaria para un cliente
        Task<bool> CreateSecondaryAccountAsync(CreateSavingsAccountDto dto);

        // Cancela una cuenta secundaria (deja balance en cero)
        Task<bool> CancelSecondaryAccountAsync(Guid accountId);

        Task<SavingsAccountSummaryDto?> GetAccountSummaryAsync(Guid accountId);

        Task<Guid?> GetAccountIdByAccountNumberAsync(string accountNumber);

        //Gestion de cuneta de ahorro termina aqui

        //metodo para obtener las cuentas activas del cliente:

        Task<List<SavingsAccountDto>> GetActiveByUserIdAsync(Guid userId);

        Task<SavingsAccountDto?> GetByAccountNumberAsync(string accountNumber);

        // para el home de administrdor

        Task<List<SavingsAccountDto>> GetAllSavingsAccountsAsync();


        Task<bool> ActivatePrimaryAccountAsync(Guid userId);


        Task<bool> DeactivatePrimaryAccountAsync(Guid userId);


        Task<bool> AddBalanceToPrimaryAccountAsync(Guid userId, decimal amount);



    }
}
