using Denarius.Domain.Entities;

namespace Denarius.Application.Results;

public sealed record TagResult
{
    public string Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;

    public TagResult(Tag tag)
    {
        Id = tag.Id.ToString();
        Name = tag.Name.Value;
        Color = tag.Color.Value;
    }
}
