namespace Denarius.Domain.Exceptions;

public class InvalidCurrencyCodeException : DomainException
{
    public InvalidCurrencyCodeException() : base("Currency code must be exactly 3 uppercase letters (ISO 4217).") { }
}
