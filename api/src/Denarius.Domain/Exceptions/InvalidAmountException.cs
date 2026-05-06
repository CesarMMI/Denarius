namespace Denarius.Domain.Exceptions;

public class InvalidAmountException : DomainException
{
    public InvalidAmountException() : base("Amount must be a positive value.") { }
}
