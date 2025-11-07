using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Entities
{
    public class UserAccount : IdentityUser
    {
        // Campos del documento funcional
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string DocumentNumber { get; set; } // La Cédula
        public bool IsActive { get; set; } // Estado de la cuenta

        // Trazabilidad
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relación de Negocio (solo para CLIENTES)
        // Se relaciona con una lista de cuentas, si un cliente puede tener varias

    }

}
