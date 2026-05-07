using Denarius.Application.Inputs.Categories;
using Denarius.Application.Interfaces.UseCases.Categories;
using Denarius.Application.Outputs.Categories;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Categories;

public class ListCategoriesUseCase(ICategoryRepository categoryRepository)
    : IListCategoriesUseCase
{
    public async Task<IEnumerable<CategoryOutput>> Execute(ListCategoriesInput input)
    {
        var categories = await categoryRepository.ListByUserAsync(input.UserId, input.Type);
        return categories.Select(CategoryOutput.FromEntity);
    }
}
