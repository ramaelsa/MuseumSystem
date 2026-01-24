using Microsoft.AspNetCore.Identity;

namespace MuseumSystem
{
    public static class DbSeeder
    {
        public static async Task SeedDefaultData(IServiceProvider service)
        {
            var userManager = service.GetService<UserManager<IdentityUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // Create the "Admin" role if it doesn't exist yet
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Define our Admin's credentials
            var adminEmail = "admin@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Create the user object
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                // Create the user in the database with the password
                var result = await userManager.CreateAsync(adminUser, "admin123");

                if (result.Succeeded)
                {
                    // 3. Link this specific user to the "Admin" role
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}