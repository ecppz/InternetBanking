
namespace Application.ViewModels.Beneficiary
{
    public class BeneficiaryViewModel
    {
        public Guid Id { get; set; } // ID del registro Beneficiary
        public Guid OwnerUserId { get; set; } // Usuario que lo agregó
        public string BeneficiaryAccountNumber { get; set; } = null!; // Cuenta destino
        public Guid BeneficiaryUserId { get; set; } // Usuario dueño de la cuenta
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}
