using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Exceptions.Accounts;

public class InvalidDeltaException : DomainException
{
    public InvalidDeltaException() : base("Delta cannot be zero.") { }
}
