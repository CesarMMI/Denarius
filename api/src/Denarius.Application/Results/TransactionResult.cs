using Denarius.Domain.Entities;
using Denarius.Domain.Enums;

namespace Denarius.Application.Results;

public sealed record TransactionResult
{
    public Guid Id { get; init; }
    public string Title { get; init; }
    public DateOnly Date { get; init; }
    public decimal Amount { get; init; }
    public string CurrencyCode { get; init; }
    public ETransactionType Type { get; init; }
    public Guid AccountId { get; init; }
    public Guid? TagId { get; init; }

    public TransactionResult(Transaction transaction)
    {
        Id = transaction.Id;
        Title = transaction.Title.Value;
        Date = transaction.Date;
        Amount = transaction.Amount.Value;
        CurrencyCode = transaction.Amount.Code;
        Type = transaction.Type;
        AccountId = transaction.AccountId;
        TagId = transaction.TagId;
    }
}