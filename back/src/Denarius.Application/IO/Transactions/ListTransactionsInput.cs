namespace Denarius.Application.IO.Transactions;

public record ListTransactionsInput
{
    public string? Description { get; init; }
    public DateTime? DateRef { get; init; }
    public TransactionType Type { get; init; }
    public Guid? CategoryId { get; init; }
    public TransactionOrderField OrderBy { get; init; }
    public bool Ascending { get; init; }

    public ListTransactionsInput(string? description = null, DateTime? dateRef = null, TransactionType type = TransactionType.All, Guid? categoryId = null, TransactionOrderField orderBy = TransactionOrderField.Date, bool ascending = false)
    {
        Description = description;
        DateRef = dateRef;
        Type = type;
        CategoryId = categoryId;
        OrderBy = orderBy;
        Ascending = ascending;
    }
}
