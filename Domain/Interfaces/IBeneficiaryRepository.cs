using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IBeneficiaryRepository : IGenericRepository<Beneficiary>
    {
        // Verifica si ya existe un beneficiario con ese número de cuenta para ese usuario
        Task<bool> ExistsAsync(Guid ownerUserId, string beneficiaryAccountNumber);

        // Obtiene todos los beneficiarios registrados por un usuario
        Task<List<Beneficiary>> GetByOwnerUserIdAsync(Guid ownerUserId);

        // Elimina un beneficiario por su ID y el ID del dueño (para evitar borrado cruzado)
        Task<bool> DeleteByIdAndOwnerAsync(Guid beneficiaryId, Guid ownerUserId);

        // Opcional: Obtiene un beneficiario específico por cuenta y dueño
        Task<Beneficiary?> GetByAccountNumberAndOwnerAsync(Guid ownerUserId, string beneficiaryAccountNumber);
    }
}
