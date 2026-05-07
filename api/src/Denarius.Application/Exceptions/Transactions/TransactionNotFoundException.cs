namespace Denarius.Application.Exceptions.Transactions;

public class TransactionNotFoundException : NotFoundException
{
    public TransactionNotFoundException(Guid transactionId)
        : base("Transaction", transactionId) { }
}
