namespace Denarius.Application.IO.Transactions;

public record CreateTransactionInput
{
    public string? Description { get; init; }
    public DateTime Date { get; init; }
    public decimal Value { get; init; }
    public Guid CategoryId { get; init; }

    public CreateTransactionInput(string? description, DateTime date, decimal value, Guid categoryId)
    {
        Description = description;
        Date = date;
        Value = value;
        CategoryId = categoryId;
    }
}
