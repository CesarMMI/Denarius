using Denarius.Domain.Enums;

namespace Denarius.Application.Inputs.Categories;

public record CreateCategoryInput(Guid UserId, string Name, string Color, CategoryType Type);
