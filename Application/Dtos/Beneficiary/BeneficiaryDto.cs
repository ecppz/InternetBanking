
namespace Application.Dtos.Beneficiary
{
    public class BeneficiaryDto
    {
        public Guid Id { get; set; }

        // Usuario que agregó este beneficiario
        public Guid OwnerUserId { get; set; }

        // Número de cuenta del beneficiario
        public string BeneficiaryAccountNumber { get; set; } = null!;

        // Usuario al que pertenece la cuenta beneficiaria
        public Guid BeneficiaryUserId { get; set; }

        // Nombre y apellido del titular de la cuenta beneficiaria
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}
