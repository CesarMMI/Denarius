namespace Denarius.Application.Inputs.Transactions;

public record DeleteTransactionInput(Guid UserId, Guid TransactionId);
