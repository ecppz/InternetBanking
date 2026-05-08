namespace Application.Dtos.Commerce
{
    public class CommerceDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Rnc { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
