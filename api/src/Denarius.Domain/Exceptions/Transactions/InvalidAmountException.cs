using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Exceptions.Transactions;

public class InvalidAmountException : DomainException
{
    public InvalidAmountException() : base("Amount must be a positive value.") { }
}
