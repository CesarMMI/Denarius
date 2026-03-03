namespace Denarius.Domain.Exceptions.Transaction;

public class InvalidTransactionTypeException() : TransactionException("The transaction type is not a valid.")
{
}