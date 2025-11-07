namespace Application.Dtos.User
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string DocumentNumber { get; set; }
        public bool? isVerified { get; set; }
        public required string Role { get; set; }
        public decimal? CurrentBalancce { get; set; }

        public bool IsActive { get; set; }
    }
}
