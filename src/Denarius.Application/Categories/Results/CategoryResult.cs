using Denarius.Domain.Enums;
using Denarius.Domain.Models;

namespace Denarius.Application.Categories.Results;

public class CategoryResult
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ECategoryType Type { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class CategoryResultExtensions
{
    public static CategoryResult ToCategoryResult(this Category category)
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
