using Denarius.Application.IO.Transactions;
using Denarius.Domain.Entities;

namespace Denarius.Application.IO.Reports;

/// <summary>A transaction as the reports show it: the sign of its value becomes its type, and the amount is positive.</summary>
public record MonthlyTransactionOutput
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public string? Description { get; init; }
    public string CategoryName { get; init; }
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }

    public MonthlyTransactionOutput(Transaction transaction, string categoryName)
    {
        Id = transaction.Id;
        Date = transaction.Date;
        Description = transaction.Description;
        CategoryName = categoryName;
        Type = transaction.Value > 0 ? TransactionType.In : TransactionType.Out;
        Amount = Math.Abs(transaction.Value);
    }
}
