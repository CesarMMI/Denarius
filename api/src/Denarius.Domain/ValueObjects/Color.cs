using System.Text.RegularExpressions;
using Denarius.Domain.Exceptions.Color;

namespace Denarius.Domain.ValueObjects;

public sealed partial class Color : ValueObject
{
    [GeneratedRegex(@"^#([A-Fa-f0-9]{3,4}|[A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$", RegexOptions.Compiled)]
    private static partial Regex GenerateColorRegex();

    private static readonly Regex ColorRegex = GenerateColorRegex();

    public string Value { get; init; }

    public Color(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new EmptyColorException();
        if (!value.StartsWith('#')) value = '#' + value;
        if (!ColorRegex.IsMatch(value)) throw new InvalidColorException();
        Value = NormalizeHexCode(value);
    }

    private static string NormalizeHexCode(string value)
    {
        var hex = value.Replace("#", "");
        
        if (hex.Length <= 4)
        {
            var chars = hex.ToCharArray();
            hex = string.Concat(chars.Select(c => $"{c}{c}"));
        }
        if (hex.Length == 6) hex += "ff";

        return "#" + hex.ToLowerInvariant();
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}