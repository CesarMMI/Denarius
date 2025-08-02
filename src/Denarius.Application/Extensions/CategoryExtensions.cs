using Denarius.Application.Domain.Results.Categories;
using Denarius.Domain.Models;

namespace Denarius.Application.Extensions;

internal static class CategoryExtensions
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