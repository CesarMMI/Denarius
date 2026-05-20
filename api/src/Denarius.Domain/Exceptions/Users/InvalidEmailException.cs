namespace Denarius.Domain.Exceptions.Users;

public class InvalidEmailException : DomainException
{
    public InvalidEmailException() : base("Email is not valid.") { }
}
