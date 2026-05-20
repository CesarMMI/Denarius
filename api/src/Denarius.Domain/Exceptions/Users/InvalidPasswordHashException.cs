namespace Denarius.Domain.Exceptions.Users;

public class InvalidPasswordHashException : DomainException
{
    public InvalidPasswordHashException() : base("Password hash cannot be null or empty.") { }
}
