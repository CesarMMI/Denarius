namespace Denarius.Application.Commands.Accounts;

public sealed record CreateAccountCommand
{
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public decimal? Balance { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
}