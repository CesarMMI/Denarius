namespace Denarius.Application.Extensions;

internal static class IdExtensions
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
