namespace Denarius.Domain.Exceptions;

public class InvalidCategoryException : DomainException
{
    public InvalidCategoryException(string message) : base(message) { }
}
