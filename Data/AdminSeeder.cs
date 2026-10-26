
using CYBP.Models;
using Microsoft.AspNetCore.Identity;

namespace CYBP.Data;

public static class AdminSeeder
{
    public static async Task SeedAdminAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        var userManager = serviceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager = serviceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        const string adminRole = "Administrator";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            throw new InvalidOperationException(
                "Rola Administrator nie istnieje.");
        }

        // ADMIN — hasło z konfiguracji
        const string adminLogin = "admin1@cybp.local";
        var password = configuration["CYBPP"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Ustaw zmienną środowiskową CYBPP.");
        }

        var admin = await userManager.FindByNameAsync(adminLogin);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminLogin,
                Email = "admin1@cybp.local",
                EmailConfirmed = true,
                MustChangePassword = false,
                IsActive = true,
                PasswordChangedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Nie udało się utworzyć ADMIN: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, adminRole))
        {
            var result = await userManager.AddToRoleAsync(
                admin, adminRole);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Nie udało się przypisać roli ADMIN: {errors}");
            }
        }

        // ADMIN2 — statyczne hasło testowe
        const string secondAdminLogin = "admin2@cybp.local";
        const string secondAdminPassword = "Admin1+Admin1+";

        var secondAdmin = await userManager
            .FindByNameAsync(secondAdminLogin);

        if (secondAdmin is null)
        {
            secondAdmin = new ApplicationUser
            {
                UserName = secondAdminLogin,
                Email = "admin2@cybp.local",
                EmailConfirmed = true,
                MustChangePassword = false,
                IsActive = true,
                PasswordChangedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(
                secondAdmin, secondAdminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Nie udało się utworzyć ADMIN2: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(
            secondAdmin, adminRole))
        {
            var result = await userManager.AddToRoleAsync(
                secondAdmin, adminRole);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Nie udało się przypisać roli ADMIN2: {errors}");
            }
        }
    }
}
