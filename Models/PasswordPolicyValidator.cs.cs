
using System.Globalization;
using System.Text.RegularExpressions;


namespace CYBP.Models;

public static class PasswordPolicyValidator
{
    public static bool IsValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
        //dodatki do hasel
        // Hasło nie może zawierać roku 1950–2030.
        for (int year = 1950; year <= 2030; year++)
        {
            if (password.Contains(
                year.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
            {
                return false;
            }
        }

        // formaty datt
        string[] formats =
        {
            "dd.MM.yyyy",
            "d.M.yyyy",
            "dd-MM-yyyy",
            "d-M-yyyy",
            "yyyy-MM-dd",
            "yyyy.MM.dd",
            "yyyy/MM/dd",
            "dd/MM/yyyy",
            "d/M/yyyy"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(
                password,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
            {
                return false;
            }
        }

        return true;
    }
}
