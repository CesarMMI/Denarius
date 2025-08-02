using Denarius.Domain.Enums;

namespace Denarius.Application.Domain.Results.Categories;

public readonly struct CategoryResult
{
    public int Id { get; init; }
    public string Name { get; init; }
    public ECategoryType Type { get; init; }
    public string? Color { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
