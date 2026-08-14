using Denarius.Application.IO.Categories;

namespace Denarius.Application.UseCases.Categories.List;

public interface IListCategoriesUseCase : IUseCase<ListCategoriesInput, Task<IEnumerable<CategoryOutput>>>
{
}
