namespace Denarius.Application.Shared.Validators;

internal static class NumberValidators
{
    public static bool IsValidInt(this int value)
    {
        return value >= 0;
    }
}
