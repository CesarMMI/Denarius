using Denarius.Application.IO.Categories;

namespace Denarius.Application.UseCases.Categories.Update;

public interface IUpdateCategoryUseCase : IUseCase<(Guid Id, UpdateCategoryInput Input), Task<CategoryOutput>>
{
}
