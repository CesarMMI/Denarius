using Denarius.Application.Domain.Results.Transactions;
using Denarius.Domain.Models;

namespace Denarius.Application.Extensions;

internal static class TransactionExtensions
{
    public static TransactionResult ToResult(this Transaction transaction)
    {
        return new TransactionResult
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Description = transaction.Description,
            AccountId = transaction.AccountId,
            CategoryId = transaction.CategoryId,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }
}
