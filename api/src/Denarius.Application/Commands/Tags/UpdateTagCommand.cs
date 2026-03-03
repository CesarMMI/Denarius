namespace Denarius.Application.Commands.Tags;

public sealed class UpdateTagCommand(string id, string? name, string? color) : IdCommand(id)
{
    public string? Name { get; init; } = name;
    public string? Color { get; init; } = color;
}