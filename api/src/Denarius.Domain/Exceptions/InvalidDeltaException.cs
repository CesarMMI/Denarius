namespace Denarius.Domain.Exceptions;

public class InvalidDeltaException : DomainException
{
    public InvalidDeltaException() : base("Delta cannot be zero.") { }
}
