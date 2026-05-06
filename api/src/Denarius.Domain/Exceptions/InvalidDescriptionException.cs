namespace Denarius.Domain.Exceptions;

public class InvalidDescriptionException : DomainException
{
    public InvalidDescriptionException() : base("Description cannot be null or empty.") { }
}
