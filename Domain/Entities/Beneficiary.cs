namespace Domain.Entities
{
    public class Beneficiary
    {
        // Identificador único del registro de beneficiario
        public Guid Id { get; set; }

        // FK al ID del cliente que guarda este beneficiario (Dueño de la lista)
        public Guid OwnerUserId { get; set; }

        // Campo CRÍTICO: Número de la cuenta del tercero a la que se transfiere
        // Este campo es requerido en la validación al agregar
        public required string BeneficiaryAccountNumber { get; set; }

        // ID del usuario al que pertenece la cuenta Beneficiaria (Útil para trazabilidad)
        public Guid BeneficiaryUserId { get; set; }

        // Nombre y Apellido del titular de la cuenta Beneficiaria (Se guardan para el listado)
        public string? Name { get; set; }
        public string? LastName { get; set; }
    }
}
