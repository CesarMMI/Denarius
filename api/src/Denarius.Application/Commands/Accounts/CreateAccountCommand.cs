namespace Denarius.Application.Commands.Accounts;

public sealed class CreateAccountCommand(string? name, string? color, decimal? balance, string? currencyCode) : Command
{
    public string? Name { get; init; } = name;
    public string? Color { get; init; } = color;
    public decimal? Balance { get; init; } = balance;
    public string? CurrencyCode { get; init; } = currencyCode;
}