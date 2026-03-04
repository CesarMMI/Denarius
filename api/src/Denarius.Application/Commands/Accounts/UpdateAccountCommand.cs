namespace Denarius.Application.Commands.Accounts;

public sealed record UpdateAccountCommand
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}