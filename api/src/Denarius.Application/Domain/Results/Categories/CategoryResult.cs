using Denarius.Domain.Enums;
using Denarius.Domain.Models;

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

internal static class CategoryResultExtensions
{
    public static CategoryResult ToResult(this Category category)
    {
        return new CategoryResult
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            Color = category.Color,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}