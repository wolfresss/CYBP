
using System.ComponentModel.DataAnnotations;

namespace CYBP.Models;

public class AdminCreateUserViewModel
{
    [Required(ErrorMessage = "Podaj login.")]
    [StringLength(256)]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Podaj adres e-mail.")]
    [EmailAddress(ErrorMessage = "Niepoprawny adres e-mail.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Podaj hasło początkowe.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Wybierz rolę.")]
    public string Role { get; set; } = "Czytelnik";
}
