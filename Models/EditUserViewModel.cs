
using System.ComponentModel.DataAnnotations;

namespace CYBP.Models;

public class AdminEditUserViewModel
{
    public string Id { get; set; } = "";

    [Required(ErrorMessage = "Podaj login.")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Podaj adres e-mail.")]
    [EmailAddress(ErrorMessage = "Niepoprawny adres e-mail.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Wybierz rolę.")]
    public string Role { get; set; } = "Czytelnik";
}
