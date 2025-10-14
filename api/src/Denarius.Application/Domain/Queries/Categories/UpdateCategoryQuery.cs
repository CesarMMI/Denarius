namespace Denarius.Application.Domain.Queries.Categories;

public class UpdateCategoryQuery : QueryId
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; } = string.Empty;
}
