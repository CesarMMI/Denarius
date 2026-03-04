using Denarius.Domain.Enums;

namespace Denarius.Application.Commands.Transactions;

public sealed record UpdateTransactionCommand
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Title { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public decimal Amount { get; init; }
    public ETransactionType Type { get; init; }
    public Guid AccountId { get; init; } = Guid.Empty;
    public Guid? TagId { get; init; }
}