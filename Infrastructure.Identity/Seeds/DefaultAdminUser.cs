using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity.Seeds
{
    public static class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<UserAccount> userManager)
        {
            UserAccount user = new()
            {
                Name = "admin",
                LastName = "admin",
                Email = "admin@email.com",
                EmailConfirmed = true,
                IsActive = true,
                DocumentNumber = "123456789",
                UserName = "admin",
            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if(entityUser == null)
                {
                    await userManager.CreateAsync(user, "123Pa$$word!");
                    await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
                }
            }
       
        }
    }
}
