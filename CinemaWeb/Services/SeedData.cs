using CinemaDomain.Model;
using CinemaInfrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaWeb.Services
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<CinemaDbContext>();

            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@cinema.com";
            var adminPassword = configuration["AdminPassword"];

            var identityUser = await userManager.FindByEmailAsync(adminEmail);

            if (identityUser == null)
            {
                identityUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(identityUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(identityUser, "Admin");
                }
            }

            if (identityUser != null)
            {
                if (!context.AppUsers.Any(u => u.AzureIdentityId == identityUser.Id))
                {
                    var appAdmin = new User
                    {
                        AzureIdentityId = identityUser.Id,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "System",
                        RegistrationDate = DateTime.UtcNow,
                        DateOfBirth = new DateTime(2000, 1, 1)
                    };

                    context.AppUsers.Add(appAdmin);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
