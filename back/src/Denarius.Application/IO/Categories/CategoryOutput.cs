using Denarius.Domain.Entities;

namespace Denarius.Application.IO.Categories;

public record CategoryOutput
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; private set; }
    public string Name { get; private set; }
    public string Color { get; private set; }

    public CategoryOutput(Category category)
    {
        Id = category.Id;
        CreatedAt = category.CreatedAt;
        UpdatedAt = category.UpdatedAt;
        Name = category.Name;
        Color = category.Color.HexCode;
    }
}
