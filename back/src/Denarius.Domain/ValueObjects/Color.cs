using Denarius.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Denarius.Domain.ValueObjects;

public partial class Color : ValueObject
{
    public string HexCode { get; }

    public Color(string hexCode)
    {
        if (string.IsNullOrWhiteSpace(hexCode))
            throw new DomainException("A cor não pode ser vazia.");

        var normalized = Normalize(hexCode);

        if (!HexColorRegex().IsMatch(normalized))
            throw new DomainException($"'{hexCode}' não é uma cor hexadecimal válida.");

        HexCode = normalized;
    }

    private static string Normalize(string hexCode)
    {
        var value = hexCode.Trim();

        if (value.StartsWith('#'))
            value = value[1..];

        if (value.Length == 3)
            value = $"{value[0]}{value[0]}{value[1]}{value[1]}{value[2]}{value[2]}";

        return $"#{value.ToUpperInvariant()}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return HexCode;
    }

    public override string ToString() => HexCode;

    [GeneratedRegex("^#[0-9A-F]{6}$")]
    private static partial Regex HexColorRegex();
}
