using Denarius.Domain.Enums;

namespace Denarius.Application.Inputs.Transactions;

public record CreateTransactionInput(
    Guid UserId,
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    string Description,
    DateTime Date);
