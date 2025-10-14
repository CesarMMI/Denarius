using Denarius.Domain.Enums;

namespace Denarius.Application.Domain.Queries.Categories;

public class CreateCategoryQuery : Query
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public ECategoryType Type { get; set; }
}
