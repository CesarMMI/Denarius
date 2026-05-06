namespace Denarius.Domain.Exceptions;

public class InvalidColorException : DomainException
{
    public InvalidColorException() : base("Color cannot be null or empty.") { }
}
