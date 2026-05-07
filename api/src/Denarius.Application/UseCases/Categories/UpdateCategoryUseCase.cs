using Denarius.Application.Exceptions.Categories;
using Denarius.Application.Inputs.Categories;
using Denarius.Application.Interfaces.UseCases.Categories;
using Denarius.Application.Outputs.Categories;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Categories;

public class UpdateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IUpdateCategoryUseCase
{
    public async Task<CategoryOutput> Execute(UpdateCategoryInput input)
    {
        var category = await categoryRepository.GetByIdAsync(input.CategoryId, input.UserId);

        if (category is null)
            throw new CategoryNotFoundException(input.CategoryId);

        category.Update(input.Name, input.Color);
        await categoryRepository.UpdateAsync(category);
        await unitOfWork.CommitAsync();
        return CategoryOutput.FromEntity(category);
    }
}
