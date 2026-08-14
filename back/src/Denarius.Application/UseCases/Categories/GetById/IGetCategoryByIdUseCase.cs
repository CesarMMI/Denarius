using Denarius.Application.IO.Categories;

namespace Denarius.Application.UseCases.Categories.GetById;

public interface IGetCategoryByIdUseCase : IUseCase<Guid, Task<CategoryOutput>>
{
}
