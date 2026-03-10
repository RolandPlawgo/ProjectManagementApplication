using Microsoft.AspNetCore.Identity;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagementApplication_IntegrationTests.Helpers
{
    public static class UserHelper
    {
        public static async Task SeedUsers(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            var dev = await CreateIfNotExists(userManager, "Developer", "developer@mail.com", "Qwerty1!");
            var scrumMaster = await CreateIfNotExists(userManager, "Scrummaster", "scrummaster@mail.com", "Qwerty1!", "Scrum Master");
            var productOwner = await CreateIfNotExists(userManager, "Productowner", "productowner@mail.com", "Qwerty1!", "Product Owner");

            await context.SaveChangesAsync();
        }
        public static async Task SeedUser(UserManager<ApplicationUser> userManager, ApplicationDbContext context, string firstName, string email, string? role = null)
        {
            if (role == null)
            {
                var scrumMaster = await CreateIfNotExists(userManager, firstName, email, "Qwerty1!");
            }
            else
            {
                var scrumMaster = await CreateIfNotExists(userManager, firstName, email, "Qwerty1!", role);
            }

            await context.SaveChangesAsync();
        }
        public static async Task DeleteAllUsers(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            userManager.Users.ToList().ForEach(async user =>
            {
                await userManager.DeleteAsync(user);
            });
            await context.SaveChangesAsync();
        }

        private static async Task<ApplicationUser> CreateIfNotExists(UserManager<ApplicationUser> userManager, string firstName, string email, string password, string? role = null)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = "Demo"
                };
                await userManager.CreateAsync(user, password);
                if (role != null)
                    await userManager.AddToRoleAsync(user, role);
            }
            return user;
        }
    }
}
