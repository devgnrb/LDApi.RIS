using LDApi.RIS.Models;
using Microsoft.AspNetCore.Identity;

namespace LDApi.RIS.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 🔹 1. Créer les rôles s’ils n’existent pas
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 🔹 2. Créer l’admin s’il n’existe pas
            var adminEmail = "genourobAdm@lda.local";
            var adminUserName = "GenourobAdm";
            var adminPassword = "Admin123!"; // à changer ensuite

            var admin = await userManager.FindByNameAsync(adminUserName);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                {
                    throw new Exception("Impossible de créer l’utilisateur Admin");
                }
            }

            // 🔹 3. Associer le rôle Admin
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
