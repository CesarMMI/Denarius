using Denarius.Domain.Entities;
using Denarius.Domain.Enums;

namespace Denarius.Application.Outputs.Transactions;

public record TransactionOutput(
    Guid Id,
    Guid AccountId,
    Guid? CategoryId,
    Guid? TransferPeerId,
    TransactionType Type,
    decimal Amount,
    string Description,
    DateTime Date,
    bool IsIncomingTransfer,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static TransactionOutput FromEntity(Transaction transaction) => new(
        transaction.Id,
        transaction.AccountId,
        transaction.CategoryId,
        transaction.TransferPeerId,
        transaction.Type,
        transaction.Amount,
        transaction.Description,
        transaction.Date,
        transaction.IsIncomingTransfer,
        transaction.CreatedAt,
        transaction.UpdatedAt);
}
