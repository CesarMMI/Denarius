using Denarius.Application.Exceptions.Categories;
using Denarius.Application.Inputs.Categories;
using Denarius.Application.Interfaces.UseCases.Categories;
using Denarius.Application.Outputs.Categories;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Categories;

public class GetCategoryByIdUseCase(ICategoryRepository categoryRepository)
    : IGetCategoryByIdUseCase
{
    public async Task<CategoryOutput> Execute(GetCategoryByIdInput input)
    {
        var category = await categoryRepository.GetByIdAsync(input.CategoryId, input.UserId);

        if (category is null)
            throw new CategoryNotFoundException(input.CategoryId);

        return CategoryOutput.FromEntity(category);
    }
}
