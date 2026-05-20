namespace Denarius.Domain.Exceptions;

public class InvalidNameException : DomainException
{
    public InvalidNameException(string message = "Name cannot be null or empty.") : base(message) { }
}
