namespace Denarius.Domain.Exceptions;

public class InvalidNameException : DomainException
{
    public InvalidNameException() : base("Name cannot be null or empty.") { }
}
