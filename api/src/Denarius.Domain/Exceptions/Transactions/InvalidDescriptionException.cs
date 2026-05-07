using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Exceptions.Transactions;

public class InvalidDescriptionException : DomainException
{
    public InvalidDescriptionException() : base("Description cannot be null or empty.") { }
}
