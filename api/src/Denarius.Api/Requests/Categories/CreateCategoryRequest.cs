using Denarius.Domain.Enums;

namespace Denarius.Api.Requests.Categories;

public record CreateCategoryRequest(string Name, string Color, CategoryType Type);
