namespace Denarius.Application.Inputs.Transactions;

public record UpdateTransactionInput(
    Guid UserId,
    Guid TransactionId,
    decimal Amount,
    string Description,
    Guid? CategoryId);
