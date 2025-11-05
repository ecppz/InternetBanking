using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Entities
{
    public class UserAccount : IdentityUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string DocumentNumber { get; set; }
    }
}
