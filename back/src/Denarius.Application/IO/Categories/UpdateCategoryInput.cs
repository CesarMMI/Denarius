namespace Denarius.Application.IO.Categories;

public record UpdateCategoryInput
{
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;

    public UpdateCategoryInput(string name, string color)
    {
        Name = name;
        Color = color;
    }
}
