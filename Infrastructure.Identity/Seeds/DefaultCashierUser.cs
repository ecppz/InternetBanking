using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity.Seeds
{
    public static class DefaultCahierUser
    {
        public static async Task SeedAsync(UserManager<UserAccount> userManager)
        {
            UserAccount user = new()
            {
                Name = "Cajero",
                LastName = "Cajero",
                Email = "cajero@email.com",
                EmailConfirmed = true,
                IsActive = true,
                DocumentNumber = "1234567890",
                UserName = "cajero",
            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if(entityUser == null)
                {
                    await userManager.CreateAsync(user, "123Pa$$word!");
                    await userManager.AddToRoleAsync(user, Roles.Cashier.ToString());
                }
            }
       
        }
    }
}
