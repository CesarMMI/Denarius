namespace Denarius.Domain.Exceptions;

public class InvalidTransferException : DomainException
{
    public InvalidTransferException(string message) : base(message) { }
}
