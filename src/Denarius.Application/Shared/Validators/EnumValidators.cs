namespace Denarius.Application.Shared.Validators;

internal static class EnumValidators
{
    public static bool IsValidEnum<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        return Enum.IsDefined(value);
    }
}
