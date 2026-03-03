using Denarius.Domain.Entities;
using Denarius.Domain.Enums;

namespace Denarius.Application.Results;

public sealed record TransactionResult
{
    public string Id { get; init; }
    public string Title { get; init; }
    public DateOnly Date { get; init; }
    public decimal Amount { get; init; }
    public string CurrencyCode { get; init; }
    public ETransactionType Type { get; init; }
    public string AccountId { get; init; }
    public string? TagId { get; init; }

    public TransactionResult(Transaction transaction)
    {
        Id = transaction.Id.ToString();
        Title = transaction.Title.Value;
        Date = transaction.Date;
        Amount = transaction.Amount.Value;
        CurrencyCode = transaction.Amount.Code;
        Type = transaction.Type;
        AccountId = transaction.Account.Id.ToString();
        TagId = transaction.Tag?.Id.ToString();
    }
}