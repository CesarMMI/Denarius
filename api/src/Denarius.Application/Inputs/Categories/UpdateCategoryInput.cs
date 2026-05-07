namespace Denarius.Application.Inputs.Categories;

public record UpdateCategoryInput(Guid UserId, Guid CategoryId, string Name, string Color);
