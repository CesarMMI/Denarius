using Denarius.Domain.Exceptions.Identifier;

namespace Denarius.Domain.ValueObjects;

public class Identifier : ValueObject
{
    public Guid Value { get; init; }

    public Identifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new EmptyIdentifierException();

        Guid id;
        try { id = Guid.Parse(value); }
        catch { throw new InvalidIdentifierException(); }

        Value = id;
    }

    public static Identifier New()
    {
        return new Identifier(Guid.NewGuid().ToString());
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
