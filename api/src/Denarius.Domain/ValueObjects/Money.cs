using Denarius.Domain.Exceptions.Money;

namespace Denarius.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Value { get; init; }
    public string Code { get; init; }

    public Money(decimal value, string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new EmptyCurrencyCodeException();
        Value = value;
        Code = code.ToUpperInvariant();
    }

    public static Money Zero(string code)
    {
        return new Money(0m, code);
    }

    public static Money operator +(Money a, decimal b)
    {
        return a + new Money(b, a.Code);
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Code != b.Code) throw new CurrencyCodeMismatchException("add");
        return new Money(a.Value + b.Value, a.Code);
    }

    public Money Negate()
    {
        return new Money(-Value, Code);
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