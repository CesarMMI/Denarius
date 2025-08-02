namespace Denarius.Application.Domain.Results.Transactions;

public readonly struct TransactionResult
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; }
    public int AccountId { get; init; }
    public int? CategoryId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
