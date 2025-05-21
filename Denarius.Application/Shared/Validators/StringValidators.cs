using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Denarius.Application.Shared.Validators;

internal static class StringValidators
{
    private static readonly Regex _hexColorRegex = new(@"^#(?:[0-9a-fA-F]{3}){1,2}$");

    public static bool IsValidString(this string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public static bool IsValidEmail(this string value)
    {
        if (!value.IsValidString()) return false;
        try
        {
            var mail = new MailAddress(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidColor(this string value)
    {
        if (!value.IsValidString()) return false;
        return _hexColorRegex.IsMatch(value);
    }
}