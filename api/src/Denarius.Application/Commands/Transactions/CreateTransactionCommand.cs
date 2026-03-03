using Denarius.Domain.Enums;

namespace Denarius.Application.Commands.Transactions;

public sealed class CreateTransactionCommand(
    string title,
    DateTime date,
    decimal amount,
    ETransactionType type,
    string accountId,
    string? tagId)
    : Command
{
    public string Title { get; private set; } = title;
    public DateTime Date { get; private set; } = date;
    public decimal Amount { get; private set; } = amount;
    public ETransactionType Type { get; private set; } = type;
    public string AccountId { get; set; } = accountId;
    public string? TagId { get; private set; } = tagId;
}