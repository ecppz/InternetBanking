using Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Enum.GetNames(typeof(Roles)))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
