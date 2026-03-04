namespace Denarius.Application.Commands;

public sealed record IdCommand
{
    public Guid Id { get; init; } = Guid.Empty;
};