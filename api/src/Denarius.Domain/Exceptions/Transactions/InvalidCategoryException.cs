using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Exceptions.Transactions;

public class InvalidCategoryException : DomainException
{
    public InvalidCategoryException(string message) : base(message) { }
}
