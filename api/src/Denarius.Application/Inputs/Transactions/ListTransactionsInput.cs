using Denarius.Domain.Enums;

namespace Denarius.Application.Inputs.Transactions;

public record ListTransactionsInput(
    Guid UserId,
    Guid? AccountId,
    Guid? CategoryId,
    TransactionType? Type,
    DateTime? StartDate,
    DateTime? EndDate);
