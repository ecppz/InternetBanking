namespace Application.Dtos.User
{
    public class UserResponseDto
    {
        public bool HasError { get; set; }
        public required List<string> Errors { get; set; }
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public bool IsVerified { get; set; } = false;
        public List<string> Roles { get; set; } = new();

    }
}
