using Denarius.Domain.Entities;

namespace Denarius.Application.Results;

public sealed record TagResult
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Color { get; init; }

    public TagResult(Tag tag)
    {
        Id = tag.Id;
        Name = tag.Name.Value;
        Color = tag.Color.Value;
    }
}
