
using CYBP.Models;
using Microsoft.AspNetCore.Identity;

namespace CYBP.Data;

public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(
        UserManager<ApplicationUser> manager,
        ApplicationUser user,
        string? password)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            !PasswordPolicyValidator.IsValid(password))
        {
            return Task.FromResult(
                IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidPasswordDate",
                    Description = "Hasło nie może być datą ani zawierać roku z zakresu 1950-2030."
                }));
        }

        return Task.FromResult(IdentityResult.Success);
    }
}
