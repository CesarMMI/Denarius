namespace Denarius.Application.IO.Categories;

public record ListCategoriesInput
{
    public string? Name { get; init; }
    public string? Color { get; init; }

    public ListCategoriesInput(string? name, string? color)
    {
        Name = name;
        Color = color;
    }
}
