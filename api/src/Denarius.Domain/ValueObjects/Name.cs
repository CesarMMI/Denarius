using Denarius.Domain.Exceptions.Name;

namespace Denarius.Domain.ValueObjects;

public sealed class Name : ValueObject
{
    private const int MinLength = 3;
    private const int MaxLength = 50;

    public string Value { get; init; }

    public static Name New(string value, string errorMessageName = "name")
    {
        if (string.IsNullOrWhiteSpace(value)) throw new EmptyNameException(errorMessageName);

        return value.Length switch
        {
            < MinLength => throw new MinLengthNameException(errorMessageName, MinLength),
            > MaxLength => throw new MaxLengthNameException(errorMessageName, MaxLength),
            _ => new Name { Value = value },
        };
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