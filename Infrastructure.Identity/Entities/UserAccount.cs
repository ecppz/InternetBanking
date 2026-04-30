using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Entities
{
    public class UserAccount : IdentityUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string DocumentNumber { get; set; }
        public required bool IsActive { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CommerceId { get; set; }

    }

}
