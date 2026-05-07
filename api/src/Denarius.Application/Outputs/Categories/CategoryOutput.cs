using Denarius.Domain.Entities;
using Denarius.Domain.Enums;

namespace Denarius.Application.Outputs.Categories;

public record CategoryOutput(
    Guid Id,
    string Name,
    string Color,
    CategoryType Type,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static CategoryOutput FromEntity(Category category) => new(
        category.Id,
        category.Name,
        category.Color,
        category.Type,
        category.CreatedAt,
        category.UpdatedAt);
}
