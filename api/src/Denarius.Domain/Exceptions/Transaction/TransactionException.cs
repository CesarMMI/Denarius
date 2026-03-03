namespace Denarius.Domain.Exceptions.Transaction;

public abstract class TransactionException(string message) : DomainException(message)
{
}