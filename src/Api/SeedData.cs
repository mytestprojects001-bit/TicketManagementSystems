using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using TicketManagementSystem.Infrastructure.Identity;
using Microsoft.Extensions.Logging;

namespace TicketManagementSystem.Api
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

            try
            {
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // Ensure SuperAdmin role exists
                var roleName = "SuperAdmin";
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    var rres = await roleManager.CreateAsync(role);
                    if (!rres.Succeeded) logger.LogWarning("Failed to create role {role}", roleName);
                }

                // Admin credentials
                var adminEmail = "admin@ticketmanagement.local";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = "admin",
                        NormalizedUserName = "ADMIN",
                        Email = adminEmail,
                        FullName = "Development Super Admin",
                        IsActive = true
                    };

                    var createRes = await userManager.CreateAsync(user, "Admin@123");
                    if (createRes.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, roleName);
                        logger.LogInformation("Seeded admin user {email}", adminEmail);
                    }
                    else
                    {
                        logger.LogWarning("Failed to create admin user: {errors}", string.Join(';', createRes.Errors));
                    }
                }
                else
                {
                    // ensure role assigned
                    if (!await userManager.IsInRoleAsync(adminUser, roleName))
                    {
                        await userManager.AddToRoleAsync(adminUser, roleName);
                        logger.LogInformation("Assigned role {role} to existing admin user", roleName);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger2 = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("SeedDataError");
                logger2.LogError(ex, "An error occurred seeding the DB.");
            }
        }
    }
}
