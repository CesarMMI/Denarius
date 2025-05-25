namespace Denarius.Application.Shared.Validators;

internal static class NumberValidators
{
    public static bool IsValidId(this int value)
    {
        return value > 0;
    }

    public static bool IsValidId(this int? value)
    {
        return !value.HasValue || value > 0;
    }
}
