namespace Denarius.Application.Inputs.Transactions;

public record GetTransactionByIdInput(Guid UserId, Guid TransactionId);
