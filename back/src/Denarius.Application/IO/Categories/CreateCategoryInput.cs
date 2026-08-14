namespace Denarius.Application.IO.Categories;

public record CreateCategoryInput
{
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;

    public CreateCategoryInput(string name, string color)
    {
        Name = name;
        Color = color;
    }
}
