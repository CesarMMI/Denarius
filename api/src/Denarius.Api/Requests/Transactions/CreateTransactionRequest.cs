using Denarius.Domain.Enums;

namespace Denarius.Api.Requests.Transactions;

public record CreateTransactionRequest(
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    string Description,
    DateTime Date);
