using Domain.Common.Enums;

namespace Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid OriginAccountId { get; set; }
        public Guid? DestinationAccountId { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Date { get; set; }
        public TransactionType Type { get; set; }

        // campos nuevos:

        // PROPIEDADES AGREGADAS:
        public required string Status { get; set; } // "APROBADA" o "RECHAZADA"
        public required string Beneficiary { get; set; } // Ej: Número de Cuenta Destino
        public required string Origin { get; set; } // Ej: Número de Cuenta Origen
    }

}
