namespace Denarius.Application.Commands.Tags;

public sealed class CreateTagCommand(string? name, string? color) : Command
{
    public string? Name { get; init; } = name;
    public string? Color { get; init; } = color;
}