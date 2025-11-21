
using Domain.Common.Enums;

namespace Application.Dtos.Transaction
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public Guid OriginAccountId { get; set; }
        public Guid? DestinationAccountId { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Date { get; set; }
        public TransactionType Type { get; set; }

        // MIs nuevas propiedades

        public required string Status { get; set; } // "APROBADA" o "RECHAZADA"

        public required string Beneficiary { get; set; } // Número de cuenta destino

        public required string Origin { get; set; } // Número de cuenta origen

        public string VisualType { get; set; } // "CRÉDITO" o "DÉBITO"

        public string Description { get; set; } = string.Empty;

        public bool EsPropia { get; set; } // true si OriginAccountId == cuenta actual


    }
}
