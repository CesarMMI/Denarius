namespace Denarius.Application.Commands.Accounts;

public sealed class UpdateAccountCommand(string id, string? name, string? color) : IdCommand(id)
{
    public string? Name { get; init; } = name;
    public string? Color { get; init; } = color;
}