namespace Denarius.Application.Commands;

public class IdCommand(string id) : Command
{
    public string Id { get; init; } = id;
};