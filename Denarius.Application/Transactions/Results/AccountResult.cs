using Denarius.Domain.Models;

namespace Denarius.Application.Transactions.Results;

public class TransactionResult
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public int? CategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class TransactionResultExtensions
{
    public static TransactionResult ToTransactionResult(this Transaction transaction)
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