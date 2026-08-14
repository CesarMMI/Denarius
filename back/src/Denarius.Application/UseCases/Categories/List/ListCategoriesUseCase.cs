using Denarius.Application.IO.Categories;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Categories.List;

internal class ListCategoriesUseCase(ICategoryRepository categoryRepository) : IListCategoriesUseCase
{
    public async Task<IEnumerable<CategoryOutput>> Execute(ListCategoriesInput input)
    {
        var categories = await categoryRepository.GetAllAsync(input.Name, input.Color);

        return categories.Select(category => new CategoryOutput(category));
    }
}
