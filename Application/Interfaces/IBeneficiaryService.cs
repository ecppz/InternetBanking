using Application.Dtos.Beneficiary;

namespace Application.Interfaces
{
    public interface IBeneficiaryService : IGenericService<BeneficiaryDto>
    {
        // Obtiene todos los beneficiarios registrados por el usuario actual
        Task<List<BeneficiaryDto>> GetByOwnerUserIdAsync(Guid ownerUserId);

        // Agrega un nuevo beneficiario validando existencia, unicidad y que no sea cuenta propia
        Task<(bool Success, string? ErrorMessage)> AddAsync(Guid ownerUserId, CreateBeneficiaryDto dto);

        // Elimina un beneficiario validando que pertenezca al usuario actual
        Task<bool> DeleteAsync(Guid beneficiaryId, Guid ownerUserId);

        // Verifica si un beneficiario ya está registrado para ese usuario
        Task<bool> ExistsAsync(Guid ownerUserId, string beneficiaryAccountNumber);
    }
}
