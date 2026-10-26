using Microsoft.AspNetCore.Identity;

namespace CYBP.Data
{
    public class IdentitySeeder
    {


    
        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            {
            "Administrator",
            "Audytor",
            "Redaktor",
            "Czytelnik"
        };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(
                            "; ",
                            result.Errors.Select(e => e.Description));

                        throw new InvalidOperationException(
                            $"Nie udało się utworzyć roli {role}: {errors}");
                    }
                }
            }
        }
    }

}

