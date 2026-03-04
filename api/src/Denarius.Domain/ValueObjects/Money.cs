using Denarius.Domain.Exceptions.Money;

namespace Denarius.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Value { get; init; }
    public string Code { get; init; }

    public static Money New(decimal value, string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new EmptyCurrencyCodeException();

        return new Money() { Value = value, Code = code.ToUpperInvariant() };
    }

    public static Money Zero(string code)
    {
        return New(0m, code);
    }

    public static Money operator +(Money a, decimal b)
    {
        return a + New(b, a.Code);
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Code != b.Code) throw new CurrencyCodeMismatchException("add");
        return New(a.Value + b.Value, a.Code);
    }

    public Money Negate()
    {
        return New(-Value, Code);
    }

    public override string ToString()
    {
        return $"{Code} {Value}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Code;
    }
}