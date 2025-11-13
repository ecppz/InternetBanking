using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BeneficiaryRepository : GenericRepository<Beneficiary>, IBeneficiaryRepository
    {
        public BeneficiaryRepository(InternetBankingContextDB context) : base(context) { }


    
        // Verifica si ya existe un beneficiario con ese número de cuenta para ese usuario
        public async Task<bool> ExistsAsync(Guid ownerUserId, string beneficiaryAccountNumber)
        {
            return await context.Beneficiaries
                .AnyAsync(b => b.OwnerUserId == ownerUserId && b.BeneficiaryAccountNumber == beneficiaryAccountNumber);
        }

        // Obtiene todos los beneficiarios registrados por un usuario
        public async Task<List<Beneficiary>> GetByOwnerUserIdAsync(Guid ownerUserId)
        {
            return await context.Beneficiaries
                .Where(b => b.OwnerUserId == ownerUserId)
                .ToListAsync();
        }

        // Elimina un beneficiario por su ID y el ID del dueño (para evitar borrado cruzado)
        public async Task<bool> DeleteByIdAndOwnerAsync(Guid beneficiaryId, Guid ownerUserId)
        {
            var beneficiary = await context.Beneficiaries
                .FirstOrDefaultAsync(b => b.Id == beneficiaryId && b.OwnerUserId == ownerUserId);

            if (beneficiary is null)
                return false;

            context.Beneficiaries.Remove(beneficiary);
            return await context.SaveChangesAsync() > 0;
        }

        // Opcional: Obtiene un beneficiario específico por cuenta y dueño
        public async Task<Beneficiary?> GetByAccountNumberAndOwnerAsync(Guid ownerUserId, string beneficiaryAccountNumber)
        {
            return await context.Beneficiaries
                .FirstOrDefaultAsync(b => b.OwnerUserId == ownerUserId && b.BeneficiaryAccountNumber == beneficiaryAccountNumber);
        }



    }
}