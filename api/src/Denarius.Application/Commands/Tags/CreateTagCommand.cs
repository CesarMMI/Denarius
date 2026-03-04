namespace Denarius.Application.Commands.Tags;

public sealed record CreateTagCommand
{
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}