namespace Application.Dtos.User
{
    public class SaveUserDto
    {
        public string? Id { get; set; }

        // Datos personales
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string DocumentNumber { get; set; }

        // Datos de Login/Seguridad
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

        // Datos de Autorización
        // Importante: Puedes usar el Enum Roles aquí para mayor seguridad de tipo
        public required string Role { get; set; }
        public decimal? CurrentBalance { get; set; }

        public int ? CommerceId { get; set; }
    }
}
