using Domain.Common.Enums;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {

        // Para cuenta de ahorro - Admin
        Task<List<Transaction>> GetByOriginAccountIdAsync(Guid accountId);
        Task<List<Transaction>> GetByDestinationAccountIdAsync(Guid accountId);
        Task<List<Transaction>> GetAllByAccountIdAsync(Guid accountId);
        Task<List<Transaction>> GetAllByAccountIdOrderedAsync(Guid accountId);

        //Para cajero:

        // Registra una transacción en el historial
        Task<bool> RegisterTransactionAsync(Transaction transaction);

        // Obtiene todas las transacciones filtradas por tipo (opcional si decides usarlo)
        Task<List<Transaction>> GetByTypeAsync(TransactionType type);

        // Obtiene transacciones por estado (APROBADA, RECHAZADA) — útil para auditoría
        Task<List<Transaction>> GetByStatusAsync(TransactionStatus status);


        // Para cuenta de ahorro aqui finaliza sus metodos

       

    }
}
