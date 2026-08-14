using Denarius.Application.IO.Categories;

namespace Denarius.Application.UseCases.Categories.Create;

public interface ICreateCategoryUseCase : IUseCase<CreateCategoryInput, Task<CategoryOutput>>
{
}
