namespace Denarius.Domain.Exceptions.Transaction;

public class ZeroOrNegativeTransactionAmountException()
    : TransactionException("The transaction amount must be greater than zero.")
{
}