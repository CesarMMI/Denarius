using Denarius.Domain.Entities;

namespace Denarius.Application.IO.Transactions;

public record TransactionOutput
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; private set; }
    public string? Description { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Value { get; private set; }
    public Guid CategoryId { get; private set; }

    public TransactionOutput(Transaction transaction)
    {
        Id = transaction.Id;
        CreatedAt = transaction.CreatedAt;
        UpdatedAt = transaction.UpdatedAt;
        Description = transaction.Description;
        Date = transaction.Date;
        Value = transaction.Value;
        CategoryId = transaction.CategoryId;
    }
}
