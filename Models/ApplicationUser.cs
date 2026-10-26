
using Microsoft.AspNetCore.Identity;

namespace CYBP.Models;

public class ApplicationUser : IdentityUser
{
    public bool MustChangePassword { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime? PasswordChangedAt { get; set; }

    public DateTime? PasswordExpiresAt { get; set; }

    // WARZNOSC hasla.
    public int PasswordValidityHours { get; set; } = 24;

    // czy wygaslo
    public bool IsPasswordExpired =>
        PasswordExpiresAt.HasValue
            ? DateTime.UtcNow >= PasswordExpiresAt.Value
            : PasswordChangedAt.HasValue &&
              DateTime.UtcNow >= PasswordChangedAt.Value
                  .AddHours(PasswordValidityHours);

    // Czy trza zmienic
    public bool RequiresPasswordChange =>
        MustChangePassword || IsPasswordExpired;
}
