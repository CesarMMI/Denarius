using Denarius.Domain.Enums;

namespace Denarius.Application.Inputs.Categories;

public record ListCategoriesInput(Guid UserId, CategoryType? Type);
