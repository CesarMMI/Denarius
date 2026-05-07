using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Exceptions.Accounts;

public class InvalidCurrencyCodeException : DomainException
{
    public InvalidCurrencyCodeException() : base("Currency code must be exactly 3 uppercase letters (ISO 4217).") { }
}
