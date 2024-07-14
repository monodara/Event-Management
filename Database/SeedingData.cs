using EventManagementApi.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Database
{
    public static class SeedingData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // Ensure there are roles available
                if (!context.Roles.Any())
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                    await roleManager.CreateAsync(new IdentityRole("EventProvider"));
                    await roleManager.CreateAsync(new IdentityRole("User"));
                }

                // Ensure there are users available
                if (!context.Users.Any())
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = "admin@example.com",
                        Email = "admin@example.com",
                        FullName = "Admin User"
                    };

                    await userManager.CreateAsync(adminUser, "P@ssw0rd");
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
