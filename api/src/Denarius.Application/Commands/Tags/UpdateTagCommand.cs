namespace Denarius.Application.Commands.Tags;

public sealed record UpdateTagCommand
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}