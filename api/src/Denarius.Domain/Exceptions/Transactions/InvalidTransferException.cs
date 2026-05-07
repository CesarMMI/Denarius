using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Exceptions.Transactions;

public class InvalidTransferException : DomainException
{
    public InvalidTransferException(string message) : base(message) { }
}
